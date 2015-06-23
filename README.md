##BeYourMarket##

BeYourMarket is a free open source marketplace framework built on the ASP.NET platform.
http://beyourmarket.com

[![ScreenShot](http://beyourmarket.com/images/github/beyourmarket.jpg)](http://beyourmarket.com/)

## Installation ##

The easiest way to get started is to get the source from github. Build and run the solution in Visual Studio. It will launch a setup wizard first time for installation.

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

The template file is located at folder locale/messages.pot after the solution is built. The file can be translated with any POEditor.
```

## RoadMap ##

We will build features based on community needs

## Docs ##

See [Documentation](https://beyourmarket.atlassian.net)

## Contribute to BeYourMarket ##

If you want to contribute back to BeYourMarket, please contact us.

## Found a bug? ##

Another way you can contribute to BeYourMarket is by providing issue reports [online guide for reporting issues](https://beyourmarket.helprace.com/).

## License ##

BeYourMarket is open source under MIT license. See [LICENSE](LICENSE) file for details.
