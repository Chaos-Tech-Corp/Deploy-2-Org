$(function () {
    $("input[name='random']").val(new Date().getTime());
    $("#bn-submit").on("click", function () {
        $(this).hide().siblings().show();
        $("form .form-element_error").remove();
    });

    if ($("#form-deploy").length>0) {
        $.post('/deploy/validate', function (r) {
            $.each($("[id^='apex-']"), function (ix, il) {
                $(il).html((r.apex_class && r.apex_class[ix] == 'ok') ? '<i class="fal fa-check-circle text-success"></i>' : '<i class="fal fa-times-circle text-danger"></i>');
            });

            $.each($("[id^='event-']"), function (ix, il) {
                $(il).html((r.events && r.events[ix] == 'ok') ? '<i class="fal fa-check-circle text-success"></i>' : '<i class="fal fa-times-circle text-danger"></i>');
            });

            if (r.bundle_details != null) {

                $.each($("[id^='bndl']"), function (ix, il) {
                    var t = $(il),
                        id = t.attr("id").split("-")[1];
                    t.html(r.bundle_details[id] == 'ok' ? '<i class="fal fa-check-circle text-success"></i>' : '<i class="fal fa-times-circle text-danger"></i>');
                });
            }

            if ($("cmp-details .text-danger").length <= 0) {
                $("#bn-deploy-prod").removeAttr("disabled");
                $("#bn-deploy-test").removeAttr("disabled").on("click", function () {
                    $("#input-environment").val('test');
                });
            }
        });
    }
});