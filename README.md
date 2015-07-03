##BeYourMarket [![Try online](https://img.shields.io/badge/try-demo-green.svg)](http://demo.beyourmarket.com) [![Documentation Status](https://img.shields.io/badge/documentation-1v-blue.svg)](https://beyourmarket.atlassian.net) [![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/beyourmarket/beyourmarket/blob/master/LICENSE) [![Github Issues](http://issuestats.com/github/beyourmarket/beyourmarket/badge/issue)](https://github.com/beyourmarket/beyourmarket/issues)

BeYourMarket is a free open source marketplace framework built on the ASP.NET platform.
http://beyourmarket.com

[![ScreenShot](http://beyourmarket.com/images/github/beyourmarket2.jpg)](http://beyourmarket.com/)

## Build status ##

Master Branch (`master`)

[![Build status](https://ci.appveyor.com/api/projects/status/ojc6mh88o61cvlgw/branch/master?svg=true)](https://ci.appveyor.com/project/beyourmarket/beyourmarket/branch/master)

Development Branch (`dev`)

[![Build status](https://ci.appveyor.com/api/projects/status/ojc6mh88o61cvlgw/branch/dev?svg=true)](https://ci.appveyor.com/project/beyourmarket/beyourmarket/branch/dev)

## Installation ##

[Running BeYourMarket with Visual Studio 2013](https://beyourmarket.atlassian.net/wiki/display/BYM/Installation)

## Quick Start

[Beauty and spa service Quick Start](http://www.codeproject.com/Articles/1001019/BeYourMarket-An-net-open-source-marketplace-framew)

## RoadMap ##

#### Support Plugin architecture development (July/August 2015) v. 1.1beta
- Create plugin for your own widget (e.g. add google analytics tracking)
- Create plugin for  other marketplace payment API like Braintree

#### Support Custom Theme  (August/September 2015) v. 1.2beta
- Develop your own theme

We will build features based on community needs, please raise your [suggestion](https://github.com/beyourmarket/beyourmarket/issues/new)

## Docs ##

See [Documentation](https://beyourmarket.atlassian.net)

## Community
Keep track of development and community news.

*   Follow [BeYourMarket](https://www.facebook.com/BeYourMarket) on Facebook.
*   Read and subscribe to [the
    Newsletter](http://beyourmarket.com/index.php/subscribe/).

## Translation ##
The platform is by designed with support of multiple language with i18n easily. Smart internationalization for ASP.NET based on GetText / PO ecosystem is used. The only thing you would need is to translate into your own language.

To localize text in your application, surround your strings with [[[ and ]]] markup characters to mark them as translatable.

Here's an example of localizing text "Create an account" and "Log in" in a Razor view:

```html
<ul class="nav navbar-nav">
    <li class="dropdown messages-menu hidden-xs">
        @Html.ActionLink("[[[Create an account]]]", "Register", "Account", new { area = string.Empty }, htmlAttributes: new { id = "registerLink" })
    </li>
    <li class="dropdown messages-menu hidden-xs">
        @Html.ActionLink("[[[Log in]]]", "Login", "Account", new { area = string.Empty }, htmlAttributes: new { id = "loginLink" })
    </li>
</ul>
```

The template file is located at folder locale/messages.pot after the solution is built. The file can be translated with any POEditor.

When language specific po file is translate, it can be placed in your locale folder relative to the provided language, e.g. locale/fr. If you change a PO file on the fly, i18n will update accordingly; you do not need to restart your application.

The locale-specific file must be named messages.po. For example, your locale folder structure will be similar to (three languages, fr, es, and es-MX are defined):

locale/fr/messages.po

locale/es/messages.po

locale/es-MX/messages.po

If you would like to contribute as a translator, pleasecontact us at [hello@beyourmarket.com](hello@beyourmarket.com)

## Contribute to BeYourMarket ##

If you want to contribute back to BeYourMarket, please contact us at [hello@beyourmarket.com](hello@beyourmarket.com)

## Bugs and Feature Requests ##

Another way you can contribute to BeYourMarket is by providing issue reports. [Please open a new
issue](https://github.com/beyourmarket/beyourmarket/issues/new).

## License ##

BeYourMarket is open source under MIT license. See [LICENSE](LICENSE) file for details.
