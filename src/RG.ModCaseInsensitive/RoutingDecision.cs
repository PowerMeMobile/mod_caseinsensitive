
using System;
using System.Web;
using System.Collections.Generic;
using System.IO;

namespace RG.ModCaseInsensitive {
	internal class RoutingDecision {
		public RoutingDecision() {
			RewriteNeeded = false;
			Url = "";
			PathInfo = "";
			QueryString = "";
		}

		public void CondRewrite(HttpContext ctx) {
			if ( RewriteNeeded ) {
				Common.Log("ModCaseInsensitive: rewritting [ '{0}' -> '{1}' ]", 
					ctx.Request.RawUrl,
					Url + PathInfo + QueryString
				);

				if ( PathInfo == String.Empty ) {
					ctx.RewritePath( Url + QueryString );
				} 
				else {
					ctx.RewritePath( Url, PathInfo, QueryString );
				}
			}
			else {
				Common.Log("ModCaseInsensitive: not rewritting [ '{0}' ]", ctx.Request.RawUrl);
			}
		}
		
		public bool RewriteNeeded { get; set; }
		public string Url { get; set; }
		public string PathInfo { get; set; }
		public string QueryString { get; set; }
	}
}
