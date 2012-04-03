
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
		
		public string Resolve( string url ) {
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

