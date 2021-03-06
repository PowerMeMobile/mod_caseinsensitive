
using System;
using System.Web;
using System.Collections.Generic;
using System.IO;

namespace mod_caseinsensitive
{
	internal class CaseMap {
		public string FSBase { get; private set; }
		public string URLBase { get; private set; }
		
		private IDictionary< string, string > _MCommon = new Dictionary< string, string >();
		private IDictionary< string, string > _MDefault = new Dictionary< string, string >();
		private IDictionary< string, bool > _MIgnore = new Dictionary< string, bool >();
		
		public CaseMap ( string fsBase, string urlBase ) {
			FSBase = fsBase;
			URLBase = urlBase;
			string[] ignorePaths = Config.Get().Ignore.GetUrls();
			foreach ( var s in ignorePaths ) {
				Common.Log("Adding to ignore map: '{0}'", s);
				_MIgnore[s] = true;
			}
		}
		
		private bool ShouldIgnore( string url ) {
			Common.Log("ShouldIgnore( '{0}' ) -> {1}", url, _MIgnore.ContainsKey( url ) );
			return _MIgnore.ContainsKey( url ) && _MIgnore[ url ];
		}
		
		public RoutingDecision ResolveReduced( string originalUrl, string head, string tail ) {
			Common.Log(
				"ResolveReduced( 'url: {0}', head: '{1}', tail: '{2}' )", 
				originalUrl, head, tail 
			);
			
			if ( head == HttpContext.Current.Request.ApplicationPath || head == "/" || head == "" ) {
				Common.Log( "ResolveReduced not found: '{0}'", originalUrl );
				return new RoutingDecision() {
					RewriteNeeded = false,
					Url = originalUrl,
					PathInfo = tail
				};
			}

			var urlLower = head.ToLowerInvariant();
			if ( _MDefault.ContainsKey( urlLower ) ) {
				Common.Log("ResolveReduced found default: '{0}'", urlLower);
				return new RoutingDecision() {
					RewriteNeeded = (head != _MCommon[urlLower]),
					Url = _MCommon[urlLower],
					PathInfo = tail
				};
			}
			else if ( _MCommon.ContainsKey( urlLower ) ) {
				Common.Log("ResolveReduced found common: '{0}'", urlLower);
				return new RoutingDecision() {
					RewriteNeeded = (head != _MCommon[urlLower]),
					Url = _MCommon[urlLower],
					PathInfo = tail
				};
			}
			else {
				return ResolveReduced(originalUrl,
						Path.GetDirectoryName( head ),
						Path.Combine( Path.GetFileName( head ), tail )
					);
			}
		}

		public RoutingDecision Resolve( string urlAndQS ) {
			var parts = urlAndQS.Split(new []{'?'}, 2);
			var url = parts[0];
			var qs = parts.Length == 2 ? ("?" + parts[1]) : "";
			
			var rd = ResolveReduced(url, url, "");
			rd.QueryString = qs;

            Common.Log("Resolved(OrigUrl: '{0}' -> url: '{1}', pi: '{2}', qs : '{3}' )", urlAndQS, rd.Url, rd.PathInfo, rd.QueryString);

			return rd;
		}
		
		public void BuildIndex () {
			ProcessDir( FSBase, URLBase );
		}
		
		private void StoreDir ( string url ) {
			Common.Log("Storing directory: '{0}'", url);
			_MCommon[ url ] = url + "/";
			_MCommon[ url + "/" ] = url + "/";
		}
		private void StoreReg ( string url ) {
			Common.Log("Storing regular: '{0}'", url);
			_MCommon[ url.ToLowerInvariant() ] = url;
			if ( Path.GetFileName(url).ToLowerInvariant() == "default.aspx" ) {
				var dir = System.IO.Path.GetDirectoryName( url );
				_MDefault[ dir.ToLowerInvariant() ] = url;
				_MDefault[ dir.ToLowerInvariant() + "/" ] = url;
			}

		}
		
		private void ProcessDir ( string path, string url ) {
			if ( ShouldIgnore( url ) )
				return;
			
			var dirInfo = new DirectoryInfo( path );
			StoreDir( url );
			
			var fInfos = dirInfo.GetFiles();
                        foreach (var fInfo in fInfos) {
				ProcessReg( fInfo.FullName, Path.Combine(url, fInfo.Name) );
			}
			var dInfos = dirInfo.GetDirectories();
                        foreach (var dInfo in dInfos) {
				var attrs = File.GetAttributes(dInfo.FullName);
				if ( (attrs & FileAttributes.ReparsePoint ) == 0 ) {
					ProcessDir( dInfo.FullName, Path.Combine(url, dInfo.Name) );
				}
			}
		}
		private void ProcessReg ( string path, string url ) {
			if ( ShouldIgnore( url ) )
				return;
			
			StoreReg( url );
		}
	}
}

