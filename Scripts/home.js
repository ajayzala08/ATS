//feather 
feather.replace()




//nav active
$(document).ready(function () {
    $('.toggle').click(function () {
        $('.r-nav').toggleClass('active')
    })
})




//popup sidebar close
$(document).ready(function () {
    $('.button_pop').click(function () {
        $('.sidebar').toggleClass('hide');
    })
})
$(".close").click(function () {
    $(".sidebar").removeClass("hide");
});



// scroll effect

// PURE JAVASCRIPT "Scroller" CLASS (OOP)

function Scroller(options) {
    this.options = options;
    this.button = null;
    this.stop = false;
}

Scroller.prototype.constructor = Scroller;

Scroller.prototype.createButton = function () {

    this.button = document.createElement('button');
    this.button.classList.add('scroll-button');
    this.button.classList.add('scroll-button--hidden');
    this.button.textContent = "Recheck";
    document.body.appendChild(this.button);
}

Scroller.prototype.init = function () {
    this.createButton();
    this.checkPosition();
    this.click();
    this.stopListener();
}

Scroller.prototype.scroll = function () {
    if (this.options.animate == false || this.options.animate == "false") {
        this.scrollNoAnimate();
        return;
    }
    if (this.options.animate == "normal") {
        this.scrollAnimate();
        return;
    }
    if (this.options.animate == "linear") {
        this.scrollAnimateLinear();
        return;
    }
}
Scroller.prototype.scrollNoAnimate = function () {
    document.body.scrollTop = 0;
    document.documentElement.scrollTop = 0;
}
Scroller.prototype.scrollAnimate = function () {
    if (this.scrollTop() > 0 && this.stop == false) {
        setTimeout(function () {
            this.scrollAnimate();
            window.scrollBy(0, (-Math.abs(this.scrollTop()) / this.options.normal['steps']));
        }.bind(this), (this.options.normal['ms']));
    }
}
Scroller.prototype.scrollAnimateLinear = function () {
    if (this.scrollTop() > 0 && this.stop == false) {
        setTimeout(function () {
            this.scrollAnimateLinear();
            window.scrollBy(0, -Math.abs(this.options.linear['px']));
        }.bind(this), this.options.linear['ms']);
    }
}

Scroller.prototype.click = function () {

    this.button.addEventListener("click", function (e) {
        e.stopPropagation();
        this.scroll();
    }.bind(this), false);

    this.button.addEventListener("dblclick", function (e) {
        e.stopPropagation();
        this.scrollNoAnimate();
    }.bind(this), false);

}

Scroller.prototype.hide = function () {
    this.button.classList.add("scroll-button--hidden");
}

Scroller.prototype.show = function () {
    this.button.classList.remove("scroll-button--hidden");
}

Scroller.prototype.checkPosition = function () {
    window.addEventListener("scroll", function (e) {
        if (this.scrollTop() > this.options.showButtonAfter) {
            this.show();
        } else {
            this.hide();
        }
    }.bind(this), false);
}

Scroller.prototype.stopListener = function () {

    // stop animation on slider drag
    var position = this.scrollTop();
    window.addEventListener("scroll", function (e) {
        if (this.scrollTop() > position) {
            this.stopTimeout(200);
        } else {
            //...
        }
        position = this.scrollTop();
    }.bind(this, position), false);

    // stop animation on wheel scroll down
    window.addEventListener("wheel", function (e) {
        if (e.deltaY > 0) this.stopTimeout(200);
    }.bind(this), false);
}

Scroller.prototype.stopTimeout = function (ms) {
    this.stop = true;
    // console.log(this.stop); //
    setTimeout(function () {
        this.stop = false;
        console.log(this.stop); //
    }.bind(this), ms);
}

Scroller.prototype.scrollTop = function () {
    var curentScrollTop = document.documentElement.scrollTop || document.body.scrollTop;
    return curentScrollTop;
}



// ------------------- USE EXAMPLE ---------------------
// *Set options
var options = {
    'showButtonAfter': 200, // show button after scroling down this amount of px
    'animate': "normal", // [false|normal|linear] - for false no aditional settings are needed
    // easy out effect
    'normal': { // applys only if [animate: normal] - set scroll loop "distanceLeft"/"steps"|"ms"
        'steps': 15, // more "steps" per loop => slower animation
        'ms': 1000 / 60 // less "ms" => quicker animation, more "ms" => snapy
    },
    // linear effect
    'linear': { // applys only if [animate: linear] - set scroll "px"|"ms"
        'px': 80, // more "px" => quicker your animation gets
        'ms': 1000 / 60 // Less "ms" => quicker your animation gets, More "ms" =>
    },
};
// *Create new Scroller and run it.
var scroll = new Scroller(options);
scroll.init();

//----------------------------------------------------------//

//-----------   Chat popup  -------------------------//

$(function () {
    var INDEX = 0;
    $("#chat-submit").click(function (e) {
        e.preventDefault();
        var msg = $("#chat-input").val();
        if (msg.trim() == '') {
            return false;
        }
        generate_message(msg, 'self');
        var buttons = [
            {
                name: 'Existing User',
                value: 'existing'
        },
            {
                name: 'New User',
                value: 'new'
        }
      ];
        setTimeout(function () {
            generate_message(msg, 'user');
        }, 1000)

    })

    function generate_message(msg, type) {
        INDEX++;
        var str = "";
        str += "<div id='cm-msg-" + INDEX + "' class=\"chat-msg " + type + "\">";
        str += "          <span class=\"msg-avatar\">";
        str += "            <img src=..\/img\/pexels-photo-1499327.jpeg>";
        str += "          <\/span>";
        str += "          <div class=\"cm-msg-text\">";
        str += msg;
        str += "          <\/div>";
        str += "        <\/div>";
        $(".chat-logs").append(str);
        $("#cm-msg-" + INDEX).hide().fadeIn(300);
        if (type == 'self') {
            $("#chat-input").val('');
        }
        $(".chat-logs").stop().animate({
            scrollTop: $(".chat-logs")[0].scrollHeight
        }, 1000);
    }

    function generate_button_message(msg, buttons) {
        /* Buttons should be object array 
          [
            {
              name: 'Existing User',
              value: 'existing'
            },
            {
              name: 'New User',
              value: 'new'
            }
          ]
        */
        INDEX++;
        var btn_obj = buttons.map(function (button) {
            return "              <li class=\"button\"><a href=\"javascript:;\" class=\"btn btn-primary chat-btn\" chat-value=\"" + button.value + "\">" + button.name + "<\/a><\/li>";
        }).join('');
        var str = "";
        str += "<div id='cm-msg-" + INDEX + "' class=\"chat-msg user\">";
        str += "          <span class=\"msg-avatar\">";
        str += "            <img src=\"https:\/\/image.crisp.im\/avatar\/operator\/196af8cc-f6ad-4ef7-afd1-c45d5231387c\/240\/?1483361727745\">";
        str += "          <\/span>";
        str += "          <div class=\"cm-msg-text\">";
        str += msg;
        str += "          <\/div>";
        str += "          <div class=\"cm-msg-button\">";
        str += "            <ul>";
        str += btn_obj;
        str += "            <\/ul>";
        str += "          <\/div>";
        str += "        <\/div>";
        $(".chat-logs").append(str);
        $("#cm-msg-" + INDEX).hide().fadeIn(300);
        $(".chat-logs").stop().animate({
            scrollTop: $(".chat-logs")[0].scrollHeight
        }, 1000);
        $("#chat-input").attr("disabled", true);
    }

    $(document).delegate(".chat-btn", "click", function () {
        var value = $(this).attr("chat-value");
        var name = $(this).html();
        $("#chat-input").attr("disabled", false);
        generate_message(name, 'self');
    })

    $("#chat-circle").click(function () {
        $("#chat-circle").toggle('scale');
        $(".chat-box").toggle('scale');
        $("#imgnew").css('display', 'none');
    })

    $(".chat-box-toggle").click(function () {
        $("#chat-circle").toggle('scale');
        $(".chat-box").toggle('scale');
    })



})
