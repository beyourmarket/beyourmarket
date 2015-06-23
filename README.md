##BeYourMarket##

BeYourMarket is a free open source marketplace framework built on the ASP.NET platform.
http://beyourmarket.com

[![ScreenShot](http://beyourmarket.com/images/github/beyourmarket.jpg)](http://beyourmarket.com/)

## Installation ##

[Documentation](https://beyourmarket.atlassian.net/wiki/display/BYM/Installation)

## Quick Start

[Beauty and spa service Quick Start](http://www.codeproject.com/Articles/1001019/BeYourMarket-An-net-open-source-marketplace-framew)

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

## RoadMap ##

We will build features based on community needs, please raise your [suggestion](https://github.com/beyourmarket/beyourmarket/issues/new)

## Docs ##

See [Documentation](https://beyourmarket.atlassian.net)

## Community
Keep track of development and community news.

*   Follow [BeYourMarket](https://www.facebook.com/BeYourMarket) on Facebook.
*   Read and subscribe to [the
    Newsletter](http://beyourmarket.com/index.php/subscribe/).

## Contribute to BeYourMarket ##

If you want to contribute back to BeYourMarket, please contact us at [hello@beyourmarket.com](hello@beyourmarket.com)

## Bugs and Feature Requests ##

Another way you can contribute to BeYourMarket is by providing issue reports. [Please open a new
issue](https://github.com/beyourmarket/beyourmarket/issues/new).

## License ##

BeYourMarket is open source under MIT license. See [LICENSE](LICENSE) file for details.
