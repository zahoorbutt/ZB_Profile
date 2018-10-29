jQuery(document).ready(function () {
	'use strict'; // use strict mode
    var revapi;
    revapi = jQuery('#revolution-slider').revolution({
        delay: 7000,
        startwidth: 300,
        startheight: 150,
        hideThumbs: 10,
        fullWidth: "off",
        fullScreen: "off",
        fullScreenOffsetContainer: "",
        touchenabled: "on",
        navigationType: "none",
        onHoverStop: "off",
    });
});