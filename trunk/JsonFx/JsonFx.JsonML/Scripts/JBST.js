﻿/*global JBST */
/*---------------------------------------------------------*\
	JsonML Browser Side Templates
	Copyright (c)2006-2008 Stephen M. McKamey
	Created: 2008-07-28-2337
	Modified: 2008-08-02-1501
\*---------------------------------------------------------*/

/* namespace JBST */
if ("undefined" === typeof JBST) {
	window.JBST = {};
}

// combines JBST and JSON to produce JsonML
/*JsonML*/ JBST.dataBind = function(/*JBST*/ template, /*JSON*/ data, /*int*/ index) {
	// NOTE: it is very important to add transformations to a copy of the template
	// nodes, otherwise it destroys the original template.

	// recursively applies dataBind to all nodes of the template graph
	/*object*/ function db(/*JBST*/ t, /*JSON*/ d, /*int*/ n) {
		// JBST node
		if (t) {
			if ("function" === typeof t) {
				// this corresponds to the $item parameter
				return t(
					{
						data: d,
						index: isFinite(n) ? Number(n) : -1
					});
			}

			var o;
			if (t instanceof Array) {
				// output array
				o = [];
				for (var i=0; i<t.length; i++) {
					// result
					var r = db(t[i], d, n);
					if (r instanceof Array && r.$JBST) {
						// result was multiple JsonML trees
						o = o.concat(r);
					} else if ("object" === typeof r) {
						// result was a JsonML tree
						o.push(r);
					} else {
						// must convert to string or JsonML will discard
						o.push(String(r));
					}
				}
				return o;
			}

			if ("object" === typeof t) {
				// output object
				o = {};
				// for each property in node
				for (var p in t) {
					if (t.hasOwnProperty(p)) {
						o[p] = db(t[p], d, n);
					}
				}
				return o;
			}
		}

		// rest are value types, so return node directly
		return t;
	}

	if (data instanceof Array) {
		var o = [];

		// flag container to differentiate from JsonML
		o.$JBST = true;

		for (var i=0; i<data.length; i++) {
			// apply template to each item in array
			o[i] = db(template, data[i], i);
		}
		return o;
	} else {
		// data is singular to apply template once
		return db(template, data, index);
	}
};
