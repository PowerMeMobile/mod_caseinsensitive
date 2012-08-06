
using System;
using System.Web;
using System.Collections.Generic;
using System.IO;

namespace RG.ModCaseInsensitive {
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
		
		public string[] ResolveReduced( string originalUrl, string head, string tail ) {
			Common.Log(
				"CaseMap:ResolveReduced( '{0}', '{1}', '{2}' )", 
				originalUrl, head, tail 
			);
			
			if ( head == HttpContext.Current.Request.ApplicationPath || head == "/" || head == "" ) {
				Common.Log( "CaseMap:ResolveReduced NOT FOUND: '{0}'", originalUrl );
				return new string[3]{originalUrl, String.Empty, null};
			}

			var urlLower = head.ToLowerInvariant();
			if ( _MDefault.ContainsKey( urlLower ) ) {
				Common.Log("CaseMap:ResolveReduced FOUND DEFAULT: '{0}'", urlLower);
				//return Path.Combine(_MDefault[urlLower], tail);
				return new string[3]{_MDefault[urlLower], tail, null};
			}
			else if ( _MCommon.ContainsKey( urlLower ) ) {
				Common.Log("CaseMap:ResolveReduced FOUND COMMON: '{0}'", urlLower);
				//return Path.Combine(_MCommon[urlLower], tail);
				return new string[3]{_MCommon[urlLower], tail, null};
			}
			else {
				return ResolveReduced(originalUrl,
						Path.GetDirectoryName( head ),
						Path.Combine( Path.GetFileName( head ), tail )
					);
			}
		}

		public string[] Resolve( string urlAndQS ) {
			var parts = urlAndQS.Split(new []{'?'}, 2);
			var url = parts[0];
			var qs = parts.Length == 2 ? ("?" + parts[1]) : "";
			Common.Log("CaseMap:Resolve( '{0}' )", urlAndQS);
			Common.Log("CaseMap:Resolve url: '{0}'", url);
			Common.Log("CaseMap:Resolve  qs: '{0}'", qs);
			
			//var adjusted = (ResolveReduced(url, url, "") + qs);
			//Common.Log("CaseMap:Resolve adjusted: '{0}'", adjusted);
			//return adjusted;
			
			var res = ResolveReduced(url, url, "");
			res[2] = qs;
			Common.Log("CaseMap:Resolve url:'{0}', pi:'{1}'", res[0], res[1]);

			return res;			
			
			/*
			var urlLower = url.ToLowerInvariant();
			if ( _MDefault.ContainsKey( urlLower ) ) {
				Common.Log("CaseMap:Resolve FOUND DEFAULT: '{0}'", urlLower);
				return _MDefault[urlLower];
			}
			if ( _MCommon.ContainsKey( urlLower ) ) {
				Common.Log("CaseMap:Resolve FOUND COMMON: '{0}'", urlLower);
				return _MCommon[urlLower];
			}
			Common.Log("CaseMap:Resolve NOT FOUND: '{0}'", url);
			return url;
			*/
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

