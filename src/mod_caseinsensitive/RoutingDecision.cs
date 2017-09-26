
using System;
using System.Web;
using System.Collections.Generic;
using System.IO;

namespace mod_caseinsensitive
{
	internal class RoutingDecision {
		public RoutingDecision() {
			RewriteNeeded = false;
			Url = "";
			PathInfo = "";
			QueryString = "";
		}

        public void CondRewrite(HttpContext ctx)
        {
            if (RewriteNeeded)
            {
                if (PathInfo == String.Empty)
                {
                    var newUrl = Url.TrimEnd('/') + QueryString; 
                    Common.Log("rewritting ctx.RewritePath({0})", newUrl);
                    ctx.RewritePath(newUrl);
                }
                else
                {
                    var pathInfo = PathInfo.TrimEnd('/');
                    if (!pathInfo.StartsWith("/")) pathInfo = '/' + pathInfo;
                    QueryString = QueryString.TrimStart('?');

                    // for mono 4.8 Patn info must start from / and querystring  should not contain first ?


                    Common.Log("rewritting ctx.RewritePath({0}, {1}, {2})", Url, pathInfo, QueryString);
                    ctx.RewritePath(Url, pathInfo, QueryString);
                }
            }
            else
            {
                Common.Log("not rewritting [ '{0}' ]", ctx.Request.RawUrl);
            }
        }
		
		public bool RewriteNeeded { get; set; }
		public string Url { get; set; }
		public string PathInfo { get; set; }
		public string QueryString { get; set; }
	}
}
