if (!String.prototype.supplant) {
    String.prototype.supplant = function (o) {
        return this.replace(/{([^{}]*)}/g,
            function (a, b) {
                var r = o[b];
                return typeof r === 'string' || typeof r === 'number' ? r : a;
            }
        );
    };
}

$(function () {
    var ticker = $.connection.ContextTicker; // the generated client-side hub proxy
    var $contextTable = $('#contextTable');
    var $contextTableBody = $contextTable.find('tbody');
    var rowTemplate = '<tr data-symbol="{Symbol}"><td>{Symbol}</td><td>{Name}</td><td>{Price}</td></tr>';

    function formatContext(context) {
        return $.extend(context, {
            Price: context.Price.toFixed(2)
        });
    }

    function init() {
        return ticker.server.GetAllContexts().done(function (contexts) {
            $contextTableBody.empty();

            $.each(contexts, function () {
                var context = formatContext(this);
                $contextTableBody.append(rowTemplate.supplant(context));
            });
        });
    }

    // Add client-side hub methods that the server will call
    $.extend(ticker.client, {
        updateStockPrice: function (context) {
            var displayStock = formatStock(context);
            $row = $(rowTemplate.supplant(displayStock)),
                $stockTableBody.find('tr[data-symbol=' + context.Symbol + ']').replaceWith($row);
        }
    });

    // Start the connection
    $.connection.hub.start().then(init);
});