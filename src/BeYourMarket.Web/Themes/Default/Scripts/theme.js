$(function () {
    "use strict";

    function e() {
        var e = $(window).height() - $("body > .header").height() - ($("body > footer").outerHeight() || 0);
        $(".wrapper").css("min-height", e + "px");
        var t = $(".wrapper").height();
        if (t > e){
            $(".left-section, html, body").css("min-height", t + "px");            
        }
        else {
            $(".left-section, html, body").css("min-height", e + "px")
        }
    }

    $(document).ready(function () {        
        $("[data-toggle='offcanvas']").click(function (e) {
            e.preventDefault();            
            if ($(window).width() <= 992) {
                $(".row-offcanvas").toggleClass("active");
                $(".left-section").removeClass("collapse-left");
                $(".row-offcanvas").toggleClass("relative")
            } else {
                $(".left-section").toggleClass("collapse-left");
            }
        });

        // collapse sidebar for ipad or small devices mode
        if ($(window).width() <= 1024) {
            $(".left-section").toggleClass("collapse-left");            
        }
    });    

    window.onresize = function(event) {
        //e();
    };

    window.onload = function () {
        //e();
    };

});

(function (e) {
    "use strict";
    e.fn.tree = function () {
        return this.each(function () {
            //var t = e(this).children("a").first().children("i").first();
            var t = e(this).children("a").first();            
            var n = e(this).children(".treeview-menu").first();
            var r = e(this).hasClass("active");
            if (r) {
                n.show();
                t.children(".fa-angle-left").first().removeClass("fa-angle-left").addClass("ion-arrow-down-b")
            }
            t.click(function (e) {
                e.preventDefault();
                if (r) {
                    n.slideUp();
                    r = false;
                    t.children(".ion-arrow-down-b").first().removeClass("ion-arrow-down-b").addClass("fa-angle-left");
                    t.parent("li").removeClass("active")
                } else {
                    n.slideDown();
                    r = true;
                    t.children(".fa-angle-left").first().removeClass("fa-angle-left").addClass("ion-arrow-down-b");
                    t.parent("li").addClass("active")
                }
            });
            n.find("li > a").each(function () {
                var t = parseInt(e(this).css("margin-left")) + 10;
                e(this).css({
                    "margin-left": t + "px"
                })
            })
        })
    }
})(jQuery)