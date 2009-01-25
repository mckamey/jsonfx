/*global JSON, JsonFx */
/*
	JsonFx_IO.js
	Ajax & JSON-RPC support

	Created: 2006-11-09-0120
	Modified: 2008-05-25-2253

	Copyright (c)2006-2009 Stephen M. McKamey
	Released under an open-source license: http://jsonfx.net/license
*/

/* dependency checks --------------------------------------------*/

if ("undefined" === typeof window.JSON) {
	throw new Error("JsonFx_IO.js requires json2.js");
}

/* ----------------------------------------------------*/

(function () {
	// wrapping in anonymous function so that the XHR ID list
	// will be only available as a closure, as this will not
	// modify the global namespace, and it will be shared
	var XHR_OCXs;

	if ("undefined" === typeof window.XMLHttpRequest) {

		// these IDs are as per MSDN documentation (including case)
		/*string[]*/ XHR_OCXs = !window.ActiveXObject ? [] :
			[
				"Msxml2.XMLHTTP.6.0",
				"Msxml2.XMLHttp.5.0",
				"Msxml2.XMLHttp.4.0",
				"MSXML2.XMLHTTP.3.0",
				"MSXML2.XMLHTTP",
				"Microsoft.XMLHTTP"
			];

		// XMLHttpRequest: augment browser to have "native" XHR
		/*XMLHttpRequest*/ window.XMLHttpRequest = function() {
			while (XHR_OCXs.length) {
				try {
					return new window.ActiveXObject(XHR_OCXs[0]);
				} catch (ex) {
					// remove the failed XHR_OCXs for all future requests
					XHR_OCXs.shift();
				}
			}

			// all XHR_OCXs failed		
			return null;
		};
	}
})();

/* ----------------------------------------------------*/

/* namespace JsonFx */
if ("undefined" === typeof window.JsonFx) {
	window.JsonFx = {};
}

/* namespace JsonFx.IO */
JsonFx.IO = {};

/*bool*/ JsonFx.IO.hasAjax = !!new XMLHttpRequest();

/*
	RequestOptions = {
		// HTTP Options
		async : bool,
		method : string,
		headers : Dictionary<string, string>,
		timeout : number,
		params : string,

		// callbacks
		onCreate : function(XMLHttpRequest, context){},
		onSuccess : function(XMLHttpRequest, context){},
		onFailure : function(XMLHttpRequest, context, Error){},
		onTimeout : function(XMLHttpRequest, context, Error){},
		onComplete : function(XMLHttpRequest, context){},

		// callback context
		context : object
	};
*/

/*RequestOptions*/ JsonFx.IO.onFailure = function(/*XMLHttpRequest|JSON*/ obj, /*object*/ cx, /*error*/ ex) {
	var name, msg, code;
	if (ex) {
		name = ex.name ? ex.name : "Error";
		msg = ex.message ? ex.message : "";
		code = isFinite(ex.code) ? Number(ex.code) : Number(ex.number);

		if (isFinite(code)) {
			name += " ("+code+")";
		}

		window.alert("Request Failed - "+name+":\r\n\""+msg+"\"\r\n"+obj);
	}
};

/*RequestOptions*/ JsonFx.IO.validateOptions = function(/*RequestOptions*/ options) {
	// establish defaults
	if ("object" !== typeof options) {
		options = {};
	}
	if ("boolean" !== typeof options.async) {
		options.async = true;
	}
	if ("string" !== typeof options.method) {
		options.method = "POST";
	} else {
		options.method = options.method.toUpperCase();
	}
	if ("string" !== typeof options.params) {
		options.params = null;
	}
	if ("object" !== typeof options.headers) {
		options.headers = {};
	}
	if (options.method === "POST" &&
		options.params &&
		!options.headers["Content-Type"]) {
		options.headers["Content-Type"] = "application/x-www-form-urlencoded";
	}

	// prevent server from sending 304 Not-Modified response
	// since we don't have a way to access the browser cache
	options.headers["If-Modified-Since"] = "Sun, 1 Jan 1995 00:00:00 GMT";
	options.headers["Cache-Control"] = "no-cache";
	options.headers.Pragma = "no-cache";

	if ("number" !== typeof options.timeout) {
		options.timeout = 60000;// 1 minute
	}
	if ("function" !== typeof options.onCreate) {
		options.onCreate = null;
	}
	if ("function" !== typeof options.onSuccess) {
		options.onSuccess = null;
	}
	if ("function" !== typeof options.onFailure) {
		options.onFailure = JsonFx.IO.onFailure;
	}
	if ("function" !== typeof options.onTimeout) {
		options.onTimeout = null;
	}
	if ("function" !== typeof options.onComplete) {
		options.onComplete = null;
	}
	if ("undefined" === typeof options.context) {
		options.context = null;
	}
	return options;
};

/*void*/ JsonFx.IO.sendRequest = function(
	/*string*/ url,
	/*RequestOptions*/ options) {

	// ensure defaults
	options = JsonFx.IO.validateOptions(options);

	var xhr = new XMLHttpRequest();

	if (options.onCreate) {
		// create
		options.onCreate(xhr, options.context);
	}

	if (!xhr) {
		if (options.onFailure) {
			// immediate failure: xhr wasn't created
			options.onFailure(xhr, options.context, new Error("XMLHttpRequest not supported"));
		}
		if (options.onComplete) {
			// complete
			options.onComplete(xhr, options.context);
		}
		return;
	}

	var cancel;
	if (options.timeout > 0) {
		// kill off request if takes too long
		cancel = window.setTimeout(
			function () {
				if (xhr) {
					xhr.onreadystatechange = function(){};
					xhr.abort();
					xhr = null;
				}
				if (options.onTimeout) {
					// timeout-specific handler
					options.onTimeout(xhr, options.context, new Error("Request Timeout"));
				} else if (options.onFailure) {
					// general-failure handler
					options.onFailure(xhr, options.context, new Error("Request Timeout"));
				}
				if (options.onComplete) {
					// complete
					options.onComplete(xhr, options.context);
				}
			}, options.timeout);
	}

	function onRSC() {
		/*
			var readyStates = [
					"uninitialized",
					"loading",
					"loaded",
					"interactive",
					"complete"
				];

			try { document.body.appendChild(document.createTextNode((xhr?readyStates[xhr.readyState]:"null")+";")); } catch (ex) {}
		*/
		var status, ex;
		if (xhr && xhr.readyState === 4 /*complete*/) {

			// stop the timeout
			window.clearTimeout(cancel);

			// check the status
			status = 0;
			try {
				status = Number(xhr.status);
			} catch (ex2) {
				// Firefox doesn't allow status to be accessed after xhr.abort()
			}

			if (status === 0) {
				// timeout

				// IE reports status zero when aborted
				// Firefox throws exception, which we set to zero
				// options.onTimeout has already been called so do nothing
				// timeout calls onComplete
				return;

			} else if (Math.floor(status/100) === 2) {// 200-299
				// success
				if (options.onSuccess) {
					options.onSuccess(xhr, options.context);
				}

			} else if (options.onFailure) { // status not 200-299
				// failure
				ex = new Error(xhr.statusText);
				ex.code = status;
				options.onFailure(xhr, options.context, ex);
			}

			if (options.onComplete) { // all
				// complete
				options.onComplete(xhr, options.context);
			}
			xhr = null;
		}
	}

	try {
		xhr.onreadystatechange = onRSC;
		xhr.open(options.method, url, options.async);

		if (options.headers) {
			for (var h in options.headers) {
				if (options.headers.hasOwnProperty(h) && options.headers[h]) {
					try {// Opera 8.0.0 doesn't have xhr.setRequestHeader
						xhr.setRequestHeader(h, options.headers[h]);
					} catch (ex) { }
				}
			}
		}

		if (options.method === "POST" && !options.params) {
			options.params = "";
		}
		xhr.send(options.params);

	} catch (ex2) {
		// immediate failure: exception thrown
		if (options.onFailure) {
			options.onFailure(xhr, options.context, ex2);
		}

	} finally {
		// in case immediately returns?
		onRSC();
	}
};

/* JsonRequest ----------------------------------------------------*/

if ("undefined" === typeof JsonFx.jsonReviver) {
	/*object*/ JsonFx.jsonReviver = function(/*string*/ key, /*object*/ value) {
		var a;
		if ("string" === typeof value) {
			a = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2}(?:\.\d*)?)Z$/.exec(value);
			if (a) {
				return new Date(Date.UTC(+a[1], +a[2] - 1, +a[3], +a[4], +a[5], +a[6]));
			}
		}
		return value;
	};
}

/*void*/ JsonFx.IO.sendJsonRequest = function (
	/*string*/ restUrl,
	/*RequestOptions*/ options) {

	// ensure defaults
	options = JsonFx.IO.validateOptions(options);

	options.headers.Accept = "application/json, application/jsonml+json";

	var onSuccess = options.onSuccess;
	options.onSuccess = function(/*XMLHttpRequest*/ xhr, /*object*/ context) {

		// decode response as JSON
		var json = xhr ? xhr.responseText : null;
		try {
			json = (json && "string" === typeof json) ?
				JSON.parse(json, JsonFx.jsonReviver) :
				null;

			if ("function" === typeof onSuccess) {
				onSuccess(json, context);
			}
		} catch (ex) {
			if (options.onFailure) {
				options.onFailure(xhr, context, ex);
			}
		} finally {
			// free closure references
			onSuccess = options = null;
		}
	};

	var onFailure = null;
	if (options.onFailure) {
		onFailure = options.onFailure;
		options.onFailure = function (/*XMLHttpRequest*/ xhr, /*object*/ context, /*Error*/ ex) {

			onFailure((xhr&&xhr.responseText), context, ex);

			// free closure references
			onFailure = null;
		};
	}

	JsonFx.IO.sendRequest(restUrl, options);
};

/* JSON-RPC ----------------------------------------------------*/

/*string*/ JsonFx.IO.jsonRpcPathEncode = function (/*string*/ rpcMethod, /*object|array*/ rpcParams) {
	var i, enc = encodeURIComponent, rpcUrl = "/";
	if (rpcMethod && rpcMethod !== "system.describe") {
		rpcUrl += enc(rpcMethod);
	}
	if ("object" === typeof rpcParams) {
		rpcUrl += "?";
		if (rpcParams instanceof Array) {
			for (i=0; i<rpcParams.length; i++) {
				if (i > 0) {
					rpcUrl += "&";
				}
				rpcUrl += enc(i);
				rpcUrl += "=";
				rpcUrl += enc(rpcParams[i]);
			}
		} else {
			for (var p in rpcParams) {
				if (rpcParams.hasOwnProperty(p)) {
					rpcUrl += enc(p);
					rpcUrl += "=";
					rpcUrl += enc(rpcParams[p]);
				}
			}
		}
	}
};

/*void*/ JsonFx.IO.sendJsonRpc = function(
	/*string*/ rpcUrl,
	/*string*/ rpcMethod,
	/*object|array*/ rpcParams,
	/*RequestOptions*/ options) {

	// ensure defaults
	options = JsonFx.IO.validateOptions(options);

	if (!options.headers.Accept) {
		options.headers.Accept = "application/json, application/jsonml+json";
	}

	// wrap callbacks with RPC layer
	var onSuccess = options.onSuccess;
	var onFailure = options.onFailure;

	// this calls onSuccess with the results of the method (not the RPC wrapper)
	// or it calls onFailure with the error of the method (not the RPC wrapper)
	options.onSuccess = function(/*XMLHttpRequest*/ xhr, /*object*/ cx) {

		var json = xhr ? xhr.responseText : null;
		try {
			json = ("string" === typeof json) ? JSON.parse(json, JsonFx.jsonReviver) : null;

			if (json.error) {
				if (onFailure) {
					onFailure(json, cx, json.error);
				}
			} else {
				if (onSuccess) {
					onSuccess(json.result, cx);
				}
			}

		} catch (ex) {
			if (onFailure) {
				onFailure(json, cx, ex);
			}
		}

		// free closure references
		onFailure = onSuccess = null;
	};

	// this calls onFailure with the RPC response
	options.onFailure = function(/*XMLHttpRequest*/ xhr, /*object*/ cx, /*Error*/ ex) {

		var json = xhr ? xhr.responseText : null;
		try {
			json = (json && "string" === typeof json) ?
				JSON.parse(json, JsonFx.jsonReviver) :
				null;

			if (onFailure) {
				onFailure(json, cx, ex);
			}
		} catch (ex2) {
			if (onFailure) {
				onFailure(json, cx, ex?ex:ex2);
			}
		}

		// free closure references
		onFailure = null;
	};

	if ("object" !== typeof rpcParams) {// must be object or array, else wrap in array
		rpcParams = [ rpcParams ];
	}

	var rpcRequest;
	if (options.method === "GET") {
		// GET RPC is encoded as part the URL
		rpcUrl += JsonFx.IO.jsonRpcPathEncode(rpcMethod, rpcParams);

	} else {
		// POST RPC is encoded as a JSON body
		rpcRequest = {
				jsonrpc : "2.0",
				method : rpcMethod,
				params : rpcParams,
				id : new Date().valueOf()
			};

		try {
			// JSON encode request object
			rpcRequest = JSON.stringify(rpcRequest);
		} catch (ex) {
			// if violates JSON, then fail
			if (onFailure) {
				onFailure(rpcRequest, options.context, ex);
			}
			return;
		}

		options.params = rpcRequest;
		options.headers["Content-Type"] = "application/json";
		options.headers["Content-Length"] = rpcRequest.length;
	}
	JsonFx.IO.sendRequest(rpcUrl, options);
};

/* JsonRpcService ----------------------------------------------------*/

/* Base type for generated JSON Services */
if ("undefined" === typeof JsonFx.IO.Service) {

	/* Ctor */
	JsonFx.IO.Service = function() {};

	/*string*/ JsonFx.IO.Service.appRoot = "";
	/*void*/ JsonFx.IO.Service.setAppRoot = function(/*string*/ root) {
		if (!root) {
			JsonFx.IO.Service.appRoot = "";
			return;
		}

		if (root.charAt(root.length-1) === '/') {
			root = root.substr(0, root.length-1);
		}

		JsonFx.IO.Service.appRoot = root;
	};

	/*event*/ JsonFx.IO.Service.prototype.onBeginRequest = null;

	/*event*/ JsonFx.IO.Service.prototype.onEndRequest = null;

	/*event*/ JsonFx.IO.Service.prototype.onAddCustomHeaders = null;

	/*string*/ JsonFx.IO.Service.prototype.getAddress = function() {
		if (JsonFx.IO.Service.appRoot) {
			return JsonFx.IO.Service.appRoot + this.address;
		} else {
			return this.address;
		}
	};

	/*void*/ JsonFx.IO.Service.prototype.invoke = function(
		/*string*/ rpcMethod,
		/*object*/ rpcParams,
		/*RequestOptions*/ options) {

		// ensure defaults
		options = JsonFx.IO.validateOptions(options);

		if (this.isDebug) {
			options.timeout = -1;
		}

		var self = this, onComplete = null;
		if ("function" === typeof this.onEndRequest) {
			// intercept onComplete to call onEndRequest
			onComplete = options.onComplete;
			options.onComplete = function(/*JSON*/ json, /*object*/ cx) {
				self.onEndRequest(cx);

				if (onComplete) {
					onComplete(json, cx);
				}

				// free closure references				
				self = onComplete = null;
			};
		}

		if ("function" === typeof this.onAddCustomHeaders) {
			this.onAddCustomHeaders(options.headers);
		}

		if ("function" === typeof this.onBeginRequest) {
			this.onBeginRequest(options.context);
		}

		JsonFx.IO.sendJsonRpc(this.getAddress(), rpcMethod, rpcParams, options);
	};

	// service description is callable via two methods
	/*string*/ JsonFx.IO.Service.prototype["system.describe"] = JsonFx.IO.Service.prototype.$describe =
		function(/*RequestOptions*/ options) {
			this.invoke("system.describe", null, options);
		};
}