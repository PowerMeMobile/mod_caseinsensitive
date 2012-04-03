
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
		
		public CaseMap ( string fsBase, string urlBase ) {
			FSBase = fsBase;
			URLBase = urlBase;
		}
		
		public string Resolve( string url ) {
			var urlLower = url.ToLowerInvariant();
			if ( _MDefault.ContainsKey( urlLower ) ) {
				#if DEBUG_CONSOLE
				Console.WriteLine("CaseMap:Resolve FOUND DEFAULT: '{0}'", urlLower);
				#endif
				return _MDefault[urlLower];
			}
			if ( _MCommon.ContainsKey( urlLower ) ) {
				#if DEBUG_CONSOLE
				Console.WriteLine("CaseMap:Resolve FOUND COMMON: '{0}'", urlLower);
				#endif
				return _MCommon[urlLower];
			}
			#if DEBUG_CONSOLE
			Console.WriteLine("CaseMap:Resolve NOT FOUND: '{0}'", url);
			#endif
			return url;
		}
		
		public void BuildIndex () {
			ProcessDir( FSBase, URLBase );
		}
		
		private void StoreDir ( string url ) {
			#if DEBUG_CONSOLE
			Console.WriteLine("Storing directory: '{0}'", url);
			#endif
			_MCommon[ url ] = url + "/";
			_MCommon[ url + "/" ] = url + "/";
		}
		private void StoreReg ( string url ) {
			#if DEBUG_CONSOLE
			Console.WriteLine("Storing regular: '{0}'", url);
			#endif
			_MCommon[ url.ToLowerInvariant() ] = url;
			if ( Path.GetFileName(url).ToLowerInvariant() == "default.aspx" ) {
				var dir = System.IO.Path.GetDirectoryName( url );
				_MDefault[ dir.ToLowerInvariant() ] = url;
				_MDefault[ dir.ToLowerInvariant() + "/" ] = url;
			}

		}
		
		private void ProcessDir ( string path, string url ) {
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
			StoreReg( url );
		}
	}
}

