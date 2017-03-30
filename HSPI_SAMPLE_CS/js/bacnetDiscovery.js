

$('#bacnetGlobalNetwork').fancytree({
		source: {
			url: dataServiceUrl, 
			data: {type: 'root'},
			cache: false
		},
	

		click: function(event, data){
		    
            //only if node wasn't selected before...

		    $('#bacnetPropertiesTable').empty();


		    var nodeData = data.node.data;

		    if (nodeData['type'] == "object") {

		        $.ajax({
		            type: "POST",
		            url: dataServiceUrl,
		            data: nodeData,
		            success: function (returnData) {
		                returnData = JSON.parse(returnData);
		                console.log(returnData);
		                buildHtmlTable(returnData, '#bacnetPropertiesTable');
		                //$("#bacnetGlobalNetwork").html(returnData);
		                //alert("Load was performed.");
		            }//,
		            //dataType: dataType
		        });

		    } else {

		        
		    }




		},


		// Called when a lazy node is expanded for the first time:
		lazyLoad: function(event, data){
			var node = data.node;
			var nodeData = node.data;

			if (nodeData.type == 'global_network')
			{
                //TODO: 'filter_ip_address'

			    nodeData['selected_ip_address'] = $('#bacnetGlobalNetwork__selected_ip_address').val();     //Text: "Filter by Network IP address":,  "Filter by 
			    nodeData['udp_port'] = $('#bacnetGlobalNetwork__udp_port').val();
			    nodeData['filter_device_instance'] = $('#bacnetGlobalNetwork__filter_device_instance').val();  //TODO: use checkbox?
			    nodeData['device_instance_min'] = $('#bacnetGlobalNetwork__device_instance_min').val();
			    nodeData['device_instance_max'] = $('#bacnetGlobalNetwork__device_instance_max').val();
			}

			data.result = {
			    url: dataServiceUrl,
			    data: nodeData,
			    cache: false
			};
		},


		loadChildren: function(event, data) {
			//console.log(data.node);
			return;
			data.node.visit(function(subNode){
				if( subNode.isUndefined() && subNode.isExpanded() ) {
					subNode.load();
				}
			});
		}

});


//from http://stackoverflow.com/questions/5180382/convert-json-data-to-a-html-table

// Builds the HTML Table out of myList.
function buildHtmlTable(myList, selector) {

    $(selector).empty();

    var columns = addAllColumnHeaders(myList, selector);

    for (var i = 0; i < myList.length; i++) {
        var row$ = $('<tr/>');
        for (var colIndex = 0; colIndex < columns.length; colIndex++) {
            var cellValue = myList[i][columns[colIndex]];
            if (cellValue == null) cellValue = "";
            row$.append($('<td/>').html(cellValue));
        }
        $(selector).append(row$);
    }

    $(selector).dataTable();
}

// Adds a header row to the table and returns the set of columns.
// Need to do union of keys from all records as some records may not contain
// all records.
function addAllColumnHeaders(myList, selector) {
    var columnSet = [];
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
    $(selector).append(headerTr$);

    return columnSet;
}