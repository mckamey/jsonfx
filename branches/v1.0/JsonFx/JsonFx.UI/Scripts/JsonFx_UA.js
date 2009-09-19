﻿/*global JsonFx, jQuery */
/*
	JsonFx_UA.js
	user-agent specific CSS support

	Created: 2006-06-10-1635
	Modified: 2009-01-17-1321

	Copyright (c)2006-2009 Stephen M. McKamey
	Distributed under an open-source license: http://jsonfx.net/license
*/

/* namespace JsonFx */
if ("undefined" === typeof window.JsonFx) {
	window.JsonFx = {};
}

/*Dictionary<string,string>*/ JsonFx.userAgent = {};

/* namespace JsonFx.UA */
JsonFx.UA = {

	/*Dictionary<string,string>*/ parseUserAgent: function(/*string*/ ua) {
		/*Dictionary<string,string>*/ var fxua = {};

		if (!ua) {
			return fxua;
		}
		ua = ua.toLowerCase();

		// RegExp tested against (2007-06-17 @ 1235):
		// http://www.useragentstring.com/pages/useragentstring.php
		// http://www.user-agents.org
		// http://en.wikipedia.org/wiki/User_agent
		var R_All = /\S+[\/][v]?\d+(\.\d+)*/g;
		var R_AOL = /\b(america online browser|aol)[\s\/]*(\d+(\.\d+)*)/;
		var R_MSIE = /(\bmsie|microsoft internet explorer)[\s\/]*(\d+(\.\d+)*)/;
		var R_Gecko = /rv[:](\d+(\.\d+)*).*?gecko[\/]\d+/;
		var R_Opera = /\bopera[\s\/]*(\d+(\.\d+)*)/;
		var R_MSPIE = /\b(mspie|microsoft pocket internet explorer)[\s\/]*(\d+(\.\d+)*)/;
		var R_iCab = /\bicab[\s\/]*(\d+(\.\d+)*)/;

		// do this first for all (covers most browser types)
		var i, s, b, raw = ua.match(R_All);
		if (raw) {
			for (i=0; i<raw.length; i++) {
				s = raw[i].indexOf('/');
				b = raw[i].substring(0, s);
				if (b && b !== "mozilla") {
					// shorten this common browser
					if (b === "applewebkit") {
						b = "webkit";
					}
					fxua[b] = raw[i].substr(s+1);
				}
			}
		}

		// aol uses multiple engines so continue checking
		if (R_AOL.exec(ua)) {
			fxua.aol = RegExp.$2;
		}

		// order is important as user-agents spoof each other	
		if (R_Opera.exec(ua)) {
			fxua.opera = RegExp.$1;
		} else if (R_iCab.exec(ua)) {
			fxua.icab = RegExp.$1;
		} else if (R_MSIE.exec(ua)) {
			fxua.ie = RegExp.$2;
		} else if (R_MSPIE.exec(ua)) {
			fxua.mspie = RegExp.$2;
		} else if (R_Gecko.exec(ua)) {
			fxua.gecko = RegExp.$1;
		}
		return fxua;
	},

	/*void*/ formatCssUserAgent: function (/*Dictionary<string,string>*/ fxua) {
		/*string*/ function format(/*string*/ b, /*string*/ v) {
			/*const string*/ var PREFIX = " ua-", i;

			/*string*/ var css = PREFIX+b;
			if (v) {
				v = v.replace(/\./g, '-');
				i = v.indexOf('-');
				while (i > 0) {
					// loop through chopping last '-' to end off
					// concat result onto return string
					css += PREFIX+b+'-'+v.substring(0, i);
					i = v.indexOf('-', i+1);
				}
				css += PREFIX+b+'-'+v;
			}
			return css;
		}

		var uaCss = "";

		for (var b in fxua) {
			if (b && fxua.hasOwnProperty(b)) {
				JsonFx.userAgent[b] = fxua[b];
				uaCss += format(b, fxua[b]);
			}
		}

		// assign user-agent classes
		return uaCss;
	},

	/*	Dynamically appends CSS classes to document.body based upon user-agent. */
	/*void*/ setCssUserAgent: function() {

		// calculate userAgent immediately, poll until can apply to body
		var fxua = JsonFx.UA.parseUserAgent(navigator.userAgent);
		fxua = JsonFx.UA.formatCssUserAgent(fxua);

		/*void*/ function applyCss() {
			if (document.body.className) {
				document.body.className += fxua;
			} else {
				document.body.className = fxua.substr(1);
			}
		}

		/*void*/ function applyCssPoll() {
			if (document.body) {
				// apply the classes
				applyCss();
			} else {
				// queue it up again
				setTimeout(applyCssPoll, 0);
			}
		}

		if (typeof jQuery !== "undefined") {
			jQuery(applyCss);
		} else {
			// begin polling until body exists
			applyCssPoll();
		}
	}
};