
using System;
using System.Web;
using System.Configuration;
using System.Collections.Generic;

namespace mod_caseinsensitive
{
	public class IgnoreElement : ConfigurationElement {
		[ConfigurationProperty("url", IsRequired=true)]
		public string url {
			get { return (string) this["url"]; } 
			set { this["url"] = value; }
		}
	}
	[ConfigurationCollection(typeof(IgnoreElement), CollectionType=ConfigurationElementCollectionType.AddRemoveClearMap)]
	public class IgnoreElementCollection : ConfigurationElementCollection {
		protected override ConfigurationElement CreateNewElement()
		{
			return new IgnoreElement();
		}
		protected override object GetElementKey(ConfigurationElement element)
		{
			return (element as IgnoreElement).url;
		}
		public string[] GetUrls() {
			var asObjects = base.BaseGetAllKeys();
			var asStrings = new string[asObjects.Length];
			for ( var i = 0; i < asObjects.Length; i++ ) {
				asStrings[i] = (string)asObjects[i];
			}
			return asStrings;
		}
	}
	
	public class Config : ConfigurationSection {
		public static Config _config = ConfigurationManager.GetSection("mod_caseinsensitive") as Config;
		public static Config Get() {
			return _config;
		}
		[ConfigurationProperty("debug", IsRequired = true)]
		public bool Debug {
			get { return (bool)this["debug"]; }
			set { this["debug"] = value; }
		}
		[ConfigurationProperty("ignore")]
		public IgnoreElementCollection Ignore {
			get { return (IgnoreElementCollection)this["ignore"]; }
			set { this["ignore"] = value; } 
		}
	}
}

