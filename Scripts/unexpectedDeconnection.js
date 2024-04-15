$(document).ready(function () {
    $(window).on("unload", function (e) {

        $.ajax({
            type: 'POST',
            url: '/Entrer/DeconnexionImprevue',
            async: true
        });
    });
})
