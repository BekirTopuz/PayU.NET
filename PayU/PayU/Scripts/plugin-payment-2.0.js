(function (window, undefined) {

    OpenPayU.extend({ name: 'Payment' }, {
        //cardHolderInputStyle:		
        completeCallback: undefined,
        orderCreateRequestData: {},

        ready: function () {
            OpenPayU.$('#payu-card-cardholder-placeholder').html("");
            this.renderCardName('payu-card-cardholder-placeholder');
            OpenPayU.$('#payu-card-number-placeholder').html("");
            this.renderCardNumber('payu-card-number-placeholder');
            OpenPayU.$('#payu-card-cvv-placeholder').html("");
            this.renderCardCvv('payu-card-cvv-placeholder');
        },
        setSession: function (data) {
            OpenPayU.Payment.sessionId = data;
        },
        before3DSecureBegin: function (fn) {
            this._defaultBefore3DSecureBegin = fn;
        },
        create: function (options, callback) {

            if (callback) this.completeCallback = callback;
            if (options.orderCreateRequestData) this.orderCreateRequestData = options.orderCreateRequestData;
            if (options.before3DSecureBegin) this._defaultBefore3DSecureBegin = options.before3DSecureBegin;
            if (options.orderCreateRequestUrl) this.orderCreateRequestUrl = options.orderCreateRequestUrl;
            if (OpenPayU.Payment.order_ref_no) {
                OpenPayU.Payment._beginPayment();
                return;
            }

            OPU.$.ajax({
                url: OpenPayU.Payment.orderCreateRequestUrl,
                type: 'POST',
                data: OpenPayU.Payment.orderCreateRequestData,
                success: function (data) {
                    if (data != null && data.OrderCreateResponse != null && data.OrderCreateResponse.Status != null && data.OrderCreateResponse.Status.StatusCode == 'OPENPAYU_SUCCESS') {
                        OpenPayU.Payment.order_ref_no = data.OrderCreateResponse.OrderId;
                        OpenPayU.Payment._beginPayment();
                    } else {
                        if (OpenPayU.Payment.completeCallback) {
                            OpenPayU.Payment.completeCallback.call(this, data);
                        }
                    };
                },
                error: function (data) {
                    //console.log("ERROR receive data 1");
                    //console.log(data);
                    if (OpenPayU.Payment.completeCallback) {
                        OpenPayU.Payment.completeCallback.call(this, data);
                    }

                }
            });
        },

        _beginPayment: function () {
            OpenPayU.Transport.send(OpenPayU.Payment.getData(), function (data) {
                if (data.Status.StatusCode == 'OPENPAYU_WARNING_CONTINUE_3DS') {
                    OpenPayU.Payment._defaultBefore3DSecureBegin();
                    OpenPayU.authorize3DS({ refReqId: data.reqId, url: data.Url });
                } else {
                    OPU.$('#payu-3dsecure-placeholder').remove();
                    if (OpenPayU.Payment.completeCallback) {
                        OpenPayU.Payment.completeCallback.call(this, data);
                    }
                }
            });
        },

        _defaultBefore3DSecureBegin: function () {
            //use by default to create a 3DSecure Placeholder centred on page !!			
            //maybe merchant want to define his own placeholder for pop-up
            //merchant can customize css ....

            OpenPayU.Builder.addPreloader();
            var options = {};
            var width = 700;
            var height = 500;
            var style = 'background-color:white;border:1px solid black;z-index:1002';
            //style += 'display:none';
            if (options.width) width = parseInt(options.width);
            if (options.height) width = parseInt(options.height);
            if (options.style) style = options.style;
            var margin_left = (parseInt(width / 2));
            var margin_top = (parseInt(height / 2))

            //to be sure that will don't have duplicate id's'
            OPU.$('#payu-3dsecure-placeholder').remove();
            var _div = OpenPayU.Builder.createDiv(OPU.$('body'), {
                id: 'payu-3dsecure-placeholder',
                'style': 'width: ' + width + 'px;height: ' + height + 'px;position: fixed;top: 50%;left: 50%;margin-left: -'
					+ margin_left + 'px;margin-top: -' + margin_top + 'px;' + style
            });
            //_div.show('slow');

        },

        getStyleForIframeInput: function (source) {
            var _options = "";
            var _allowedElements = [
					"font-size",
					"font-weight",
					"font-style",
					"font-size-adjust",
					"color",
					"text-transform",
					"text-decoration",
					"letter-spacing",
					"word-spacing",
					"text-align",
					"vertical-align",
					"direction",
					"text-overflow",
					"cursor",
					"padding-bottom",
					"padding-left",
					"padding-right",
					"padding-top",
					"padding",
			];

            var _removeElements = [
					"padding-bottom",
					"padding-left",
					"padding-right",
					"padding-top",
					"padding"];

            element = source[0];
            if (!element) return _options;
            // The DOM Level 2 CSS way
            if ('getComputedStyle' in window) {
                var cs = getComputedStyle(element, '');
                for (var i = 0; i < _allowedElements.length; i++) {
                    var __prop = cs.getPropertyValue(_allowedElements[i]);
                    if (__prop) {
                        _options += _allowedElements[i] + ":" + __prop + ";";

                        if (OPU.$.inArray(_allowedElements[i], _removeElements) >= 0) {
                            element.style.setProperty(_allowedElements[i], "0", 'important');
                        }
                    }
                }
                // The IE way
            } else if ('currentStyle' in element) {
                var cs = element.currentStyle;
                for (var i = 0; i < _allowedElements.length; i++) {
                    var __prop = cs[_allowedElements[i]];
                    if (__prop) {
                        _options += _allowedElements[i] + ":" + __prop + ";";
                    }
                }

            }
            return _options;
        },

        renderCardIframeElement: function (name, id) {

            var _style = OpenPayU.Payment.getStyleForIframeInput(OPU.$('#' + id));
            if (_style) _style = "&style=" + encodeURIComponent(_style);

            var _placeholder = "";
            if (OPU.$('#' + id).attr("placeholder")) {
                _placeholder = "&placeholder=" + encodeURIComponent(OPU.$('#' + id).attr("placeholder"));
            }

            //for IE7 compatibility
            var _iframe = OpenPayU.Builder.createIframe(OPU.$('#' + id), {
                style: "padding:0;border:0;margin:0;width: 100%;height: 100%;overflow:hidden",
                scrolling: "no",
                width: '100%',
                marginheight: "0",
                marginwidth: "0",
                frameborder: "0",
                id: "iframe_" + name,
                name: "iframe_" + name,
                src: OpenPayU.Transport.config.url + "/input.html?name=" + name + _placeholder + _style
            });
        },
        renderCardName: function (id, options) {
            this.renderCardIframeElement('payu_card_cardholder', id);
        },
        renderCardNumber: function (id, options) {
            this.renderCardIframeElement('payu_card_number', id);

        },
        renderCardCvv: function (id, options) {
            this.renderCardIframeElement('payu_card_cvv', id);
        },

        getData: function (data) {
            return {
                script: "plugin-payment-2.0.js",
                module: 'payment_begin',
                order_ref_no: OpenPayU.Payment.order_ref_no,
                card_expm: OPU.$("#payu-card-expm").val(),
                card_expy: OPU.$("#payu-card-expy").val(),
                id_account: OpenPayU.Payment.id_account

            }
        },

        runEvent: function (data) {
            OPU.$('#' + data.field).triggerHandler(data.eventName);
            return false;
        }
    });

} (window));