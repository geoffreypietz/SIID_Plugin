
//from http://stackoverflow.com/questions/5180382/convert-json-data-to-a-html-table

// Builds the HTML Table out of myList.
function buildHtmlTable(myList, selector) {

    $(selector).empty();    //just to be sure

    var columns = addAllColumnHeaders(myList, selector);

    var tBody$ = $('<tbody/>');

    for (var i = 0; i < myList.length; i++) {
        var row$ = $('<tr/>');
        for (var colIndex = 0; colIndex < columns.length; colIndex++) {
            var cellValue = myList[i][columns[colIndex]];
            if (cellValue == null) cellValue = "";
            row$.append($('<td/>').html(cellValue));
        }
        $(tBody$).append(row$);
    }
    $(selector).append(tBody$);


    //$(selector).DataTable({
    //    "paging": false,
    //    "ordering": false,
    //    "info": false,
    //    "bFilter": false
    //});


    //$(selector).dataTable({ bFilter: false, bInfo: false, bPaginate: false });

    //$(selector).dataTable({ sDom: '<"H"lfr>t<"F"ip>' });



    //$(selector).dataTable({ sDom: 't' });

    $(selector).dataTable({ sDom: '<"H">t<"F">', bDestroy: true, bPaginate: false });

    
}

// Adds a header row to the table and returns the set of columns.
// Need to do union of keys from all records as some records may not contain
// all records.
function addAllColumnHeaders(myList, selector) {
    var columnSet = [];
    var tHead$ = $('<thead/>');
    var headerTr$ = $('<tr/>');

    for (var i = 0; i < myList.length; i++) {
        var rowHash = myList[i];
        for (var key in rowHash) {
            if ($.inArray(key, columnSet) == -1) {
                columnSet.push(key);
                headerTr$.append($('<th/>').html(key));
            }
        }
    }

    tHead$.append(headerTr$);
    $(selector).append(tHead$);

    return columnSet;
}


