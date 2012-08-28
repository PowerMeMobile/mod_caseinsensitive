
using System;
using System.Web;
using System.Collections.Generic;
using System.IO;

namespace RG.ModCaseInsensitive {
	public class Module : IHttpModule {
		public void Dispose () {}
		
		private CaseMap _CM = null;
		
		public void Init ( HttpApplication app ) {
			Common.Log( "ModCaseInsensitive:Init( app -> {0} )", app );
			app.BeginRequest += OnBeginRequest;
			
			AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
			string baseDir = setup.ApplicationBase;
			
			_CM = new CaseMap(baseDir, HttpContext.Current.Request.ApplicationPath);
			_CM.BuildIndex();
		}
		public void OnBeginRequest ( object sender, EventArgs ea ) {
			Common.Log( "ModCaseInsensitive:OnBeginRequest ( sender -> {0}, ea -> {1} )", sender, ea );
			var ctx = HttpContext.Current;
			
			var path = ctx.Request.RawUrl;
			//var newPath = _CM.Resolve( path );
			//Common.Log("ModCaseInsensitive: rewritting [ '{0}' -> '{1}' ]", path, newPath);
			//ctx.RewritePath( newPath );

			var rd = _CM.Resolve( path );

			rd.CondRewrite(ctx);			
		} 
	}
}

