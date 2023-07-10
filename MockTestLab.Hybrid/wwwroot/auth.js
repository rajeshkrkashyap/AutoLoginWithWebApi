window.blazorApp = {
    setAuthTokenCookie: function (token) {
        document.cookie = "AuthToken123=" + token + "; expires=" + new Date(new Date().getTime() + 7 * 24 * 60 * 60 * 1000) + "; path=/";
    },
    setCookie: function (key, value) {
        document.cookie = key + "=" + value + "; expires=" + new Date(new Date().getTime() + 7 * 24 * 60 * 60 * 1000) + "; path=/";
    },

    getCookieValue: function (cookieName) {
        var name = cookieName + "=";
        var decodedCookie = decodeURIComponent(document.cookie);
        var cookieArray = decodedCookie.split(";");

        for (var i = 0; i < cookieArray.length; i++) {
            var cookie = cookieArray[i].trim();

            if (cookie.indexOf(name) === 0) {
                return cookie.substring(name.length, cookie.length);
            }
        }
        return "";
    }
};