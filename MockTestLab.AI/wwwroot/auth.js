window.blazorApp = {
    setAuthTokenCookie: function (token) {
        document.cookie = "AuthToken=" + token + "; expires=" + new Date(new Date().getTime() + 15 * 24 * 60 * 60 * 1000) + "; path=/";
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
    },

    deleteCookie: function (cookeiName) {
        document.cookie = cookeiName + '=; Path=/; Expires=Sun, 11 Sep 1977 00:35:00 GMT;';
    }

};