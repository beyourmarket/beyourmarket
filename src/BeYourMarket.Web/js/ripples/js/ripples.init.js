(function ($) {
    // Selector to select only not already processed elements
    $.materialripples = {
        "options": {
            "withRipples": [
              ".btn:not(.btn-link)",
              ".card-image",
              ".navbar a:not(.withoutripple)",
              ".dropdown-menu a",
              ".nav-tabs a:not(.withoutripple)",
              ".withripple"              
            ].join(",")
        },
        "ripples": function (selector) {
            $((selector) ? selector : this.options.withRipples).ripples();
        },
        "init": function () {
            this.ripples();
        }
    };

})(jQuery);

$.materialripples.init();