using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc.Html;
using System.Web.Mvc;
using System.Web.Routing;
using System.Reflection;
using System.Text;

namespace System.Web.Mvc.Html
{
  public static class HtmlHelpers
  {

    public static string Disabled(this HtmlHelper helper, bool input)
    {
      return input ? "disabled='disabled'" : "";
    }

    // applies cssClass and an optional secondary class, altClass, when input is true
    public static string CssClass(this HtmlHelper helper, bool input, string cssClass="", string altClass="")
    {
      string addClass = input ? String.Format(" {0}", altClass) : "";
      return String.Format("{0}{1}", cssClass, addClass);
    }

    // creates a button control that acts as a hyperlink
    public static MvcHtmlString ActionButton(this HtmlHelper helper, string Name, string Controller, string Action, object HtmlAttributes)
    {
      UrlHelper url = new UrlHelper(helper.ViewContext.RequestContext);
      StringBuilder sb = new StringBuilder("<input type='button' ");
      
      string buttonName = Name.Replace(" ", "_").ToLower();
      string linkURL = url.Action(Action, Controller);
      RouteValueDictionary attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(HtmlAttributes);

      sb.Append(String.Format("id=\"{0}\" ", attributes.GetAttribute("id", buttonName) ));
      sb.Append(String.Format("name=\"{0}\" ", attributes.GetAttribute("name", buttonName) ));
      sb.Append(String.Format("value=\"{0}\" ", Name ));
      if (attributes.ContainsKey("id")) attributes.Remove("id");
      if (attributes.ContainsKey("name")) attributes.Remove("name");

      foreach(KeyValuePair<string, object> keyValue in attributes)
      {
        sb.Append(String.Format("{0}=\"{1}\" ", keyValue.Key, keyValue.Value));
      }

      sb.Append(String.Format("onclick=\"window.location.replace('{0}');\"", linkURL));
      sb.Append("/>");

      return MvcHtmlString.Create(sb.ToString());
    }

    // extend route value class to fetch attributes with fallback to a default value
    public static string GetAttribute(this RouteValueDictionary HtmlAttributes, string Key, string DefaultValue)
    {
      object outValue;
      bool result = HtmlAttributes.TryGetValue(Key, out outValue);
      return result ? outValue.ToString() : DefaultValue;
    }

  }
}