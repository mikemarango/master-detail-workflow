$(function () {
    $('a[data-modal=parts]').on('click', function () {
        $('#parts-modal-content').load(this.href, function () {
            $('#parts-modal').modal({ keyboard: true }, 'show');
            $('#part-choice').submit(function () {
                if ($('#part-choice').valid()) {
                    $.ajax({
                        url: this.action,
                        type: this.method,
                        data: $(this).serialize(),
                        success: function (result) {
                            if (result.success) {
                                $('#parts-modal').modal('hide');
                                location.reload();
                            } else {
                                $('#message-to-client').text('The data was not updated.');
                            }
                        },
                        error: function () {
                            $('#message-to-client').text('The web server had an error.');
                        }
                    });
                    return false;
                }
            });
        });
        return false;
    });

    $("a[data-modal=labors]").on("click", function () {
        $("#laborsModalContent").load(this.href, function () {
            $("#laborsModal").modal({ keyboard: true }, "show");

            $("#laborchoice").submit(function () {
                if ($("#laborchoice").valid()) {
                    $.ajax({
                        url: this.action,
                        type: this.method,
                        data: $(this).serialize(),
                        success: function (result) {
                            if (result.success) {
                                $("#laborsModal").modal("hide");
                                location.reload();
                            } else {
                                $("#MessageToClient").text("The data was not updated.");
                            }
                        },
                        error: function () {
                            $("#MessageToClient").text("The web server had an error.");
                        }
                    });
                    return false;
                }
            });
        });
        return false;
    });

    $('#parts-modal').on('show.bs.modal', function () {
        recalculatePart();
    });

    $('#parts-modal').on('hide.bs.modal', function () {
        location.reload();
    });

    $("#laborsModal").on("show.bs.modal", function () {
        recalculateLabor();
    });

    $("#laborsModal").on("hide.bs.modal", function () {
        location.reload();
    });
});


function recalculatePart() {
    if (!$('.deleteform').exists()) {


        var quantity = parseInt(document.getElementById('Quantity').value).toFixed(0);
        var unitPrice = parseFloat(document.getElementById('UnitPrice').value).toFixed(2);

        if (isNaN(quantity)) {
            quantity = 0;
        }

        if (isNaN(unitPrice)) {
            unitPrice = 0;
        }

        document.getElementById('Quantity').value = quantity;
        document.getElementById('UnitPrice').value = unitPrice;

        document.getElementById('ExtendedPrice').value = numberWithCommas((quantity * unitPrice).toFixed(2));
    }
}

function recalculateLabor() {
    if (!$(".deleteform").exists()) {
        var laborHours = parseFloat(document.getElementById("LaborHours").value).toFixed(2);
        var rate = parseFloat(document.getElementById("Rate").value).toFixed(2);

        if (isNaN(laborHours)) {
            laborHours = 0;
        }

        if (isNaN(rate)) {
            rate = 0;
        }

        document.getElementById("LaborHours").value = laborHours;
        document.getElementById("Rate").value = rate;

        document.getElementById("ExtendedPrice").value = numberWithCommas((laborHours * rate).toFixed(2));
    }
}

function numberWithCommas(n) {
    return n.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

jQuery.fn.exists = function () {
    return this.length > 0;
}

