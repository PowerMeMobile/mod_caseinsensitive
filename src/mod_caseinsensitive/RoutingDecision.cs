
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
                Common.Log("ModCaseInsensitive: rewritting [ '{0}' -> Url: {1}, PathInfo: {2}, QueryString: {3} ]",
                    ctx.Request.RawUrl,
                    Url, PathInfo, QueryString
                );

                if (PathInfo == String.Empty)
                {
                    if (Url.EndsWith("/") || QueryString.StartsWith("/"))
                        ctx.RewritePath(Url + QueryString);
                    else
                        ctx.RewritePath(Url + "/" + QueryString);
                }
                else
                {
                    var pathInfo = PathInfo.TrimEnd('/');
                    if (!pathInfo.StartsWith("/")) pathInfo = '/' + pathInfo;
                    QueryString = QueryString.TrimStart('?');

                    // for mono 4.8 Patn info must start from / and querystring  should not contain first ?


                    Common.Log("ModCaseInsensitive : rewritting ctx.RewritePath({0}, {1}, {2})", Url, pathInfo, QueryString);
                    ctx.RewritePath(Url, pathInfo, QueryString);
                }
            }
            else
            {
                Common.Log("ModCaseInsensitive: not rewritting [ '{0}' ]", ctx.Request.RawUrl);
            }
        }
		
		public bool RewriteNeeded { get; set; }
		public string Url { get; set; }
		public string PathInfo { get; set; }
		public string QueryString { get; set; }
	}
}
