

var selectedTreeNode = null;

$('#bacnetDiscoveryTree').fancytree({


		source: {
			url: bacnetDataServiceUrl, 
			data: {node_type: 'root'},
			cache: false
    },


        //source: [],
	

		click: function(event, data){
		    

		    if (selectedTreeNode === data.node)
		        return;
		    selectedTreeNode = data.node;

		    console.log(selectedTreeNode);

		    var nodeType = selectedTreeNode.data['node_type'];

		    if (nodeType == "global_network") {

		        $('#bacnetDiscoveryObjectProperties').css('display', 'none');
		        $('#bacnetDiscoveryFilters').css('display', 'block');

		    } else {
		        $('#bacnetDiscoveryFilters').css('display', 'none');
		        $('#bacnetDiscoveryObjectProperties').css('display', 'block');      //even if empty.  May have "Add device" button
		        
		    }



		    $('#addBacnetDeviceButtonContainer').css('display', 'none');
		    $('#addBacnetObjectButtonContainer').css('display', 'none');
		    if (isDeviceNodeSelected()) {   //device master node or device object node...
		        $('#addBacnetDeviceButtonContainer').css('display', 'block');
		    } else if (nodeType == "object") {
		        $('#addBacnetObjectButtonContainer').css('display', 'block');
		    }



		    $('#bacnetPropertiesTable').empty();
		    if (nodeType == "object") {

		        console.log(bacnetDataServiceUrl);

		        $.ajax({
		            type: "GET",
		            url: bacnetDataServiceUrl,
		            data: selectedTreeNode.data,
		            success: function (returnData) {
		                returnData = JSON.parse(returnData);
		                console.log(returnData);
		                buildHtmlTable(returnData, '#bacnetPropertiesTable');
		                $('#bacnetPropertiesTableContainer').css('display', 'block');
		                //$("#bacnetGlobalNetwork").html(returnData);
		                //alert("Load was performed.");
		            }//,
		            //dataType: dataType
		        });

		    } else {
		        $('#bacnetPropertiesTableContainer').css('display', 'none');
		    }

		},


		// Called when a lazy node is expanded for the first time:
		lazyLoad: function(event, data){
			var node = data.node;
			var nodeData = node.data;

			if (nodeData['node_type'] == 'global_network')
			{
			    var bgn = 'bacnetGlobalNetwork__';

			    //console.log($('input:radio[name=' + bgn + 'filter_ip_address]:checked').val());
			    //console.log($('input:radio[name=' + bgn + 'filter_device_instance]:checked').val());

			    nodeData['filter_ip_address'] = $('#' + bgn + 'filter_ip_address_1').is(':checked');
			    nodeData['selected_ip_address'] = $('#' + bgn + 'selected_ip_address').val();
			    nodeData['udp_port'] = $('#' + bgn + 'udp_port').val();

			    //nodeData['filter_device_instance'] = ($('input:radio[name=' + bgn + 'filter_device_instance]:checked').val() !== 0);  //0 = all devices, 1 = filter
			    nodeData['filter_device_instance'] = $('#' + bgn + 'filter_device_instance_1').is(':checked');
			    nodeData['device_instance_min'] = $('#' + bgn + 'device_instance_min').val();
			    nodeData['device_instance_max'] = $('#' + bgn + 'device_instance_max').val();

			    console.log(nodeData);

			}

			data.result = {
			    url: bacnetDataServiceUrl,
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



$('#discoverBACnetDevices').click(function() {
    

    var tree = $("#bacnetDiscoveryTree").fancytree("getTree");

    var allNetworksNode = tree.rootNode.children[0];

    allNetworksNode.resetLazy();
    allNetworksNode.load();


});



$('#addBacnetDevice').click(function () {

    addBacnetObjectNode();

});



$('#addBacnetObject').click(function () {

    addBacnetObjectNode();

});


function addBacnetObjectNode() {
    //to homeSeer, if not already present


    var bacnetObjectData = selectedTreeNode.data;


    if (isDeviceNodeSelected()) {
        bacnetObjectData.node_type = 'device';      //even if they selected the device object, we want to indicate that this is a special case
        bacnetObjectData.object_type = 8;           //these attributes weren't present before if they selected the device node directly...
        bacnetObjectData.object_instance = bacnetObjectData.device_instance;
    }



    //TODO: maybe should get instead?

    $.ajax({
        type: "POST",
        url: bacnetHomeSeerDevicePageUrl,
        data: bacnetObjectData,
        success: function (returnData) {
            //redirect to HomeSeer device edit page (whether new or existing device)
            console.log(returnData);
            window.location.href = returnData;

        }//,
        //dataType: dataType
    });




}



function isDeviceNodeSelected() {

    var nodeType = selectedTreeNode.data.node_type;

    return ((nodeType == "device") || ((nodeType == "object")  && (selectedTreeNode.data["object_type"] === 8)))

}



//function nodeDevice(node) {

//    if ((node.data["type"] == "device") || ((nodeType == "object") && (selectedTreeNode.data))) {
//        $('#addBacnetDeviceButtonContainer').css('display', 'block');
//    }



//    if ((nodeType == "device") || ((nodeType == "object") && (selectedTreeNode.data))) {
//        $('#addBacnetDeviceButtonContainer').css('display', 'block');
//    }








//}



//function nodeObject(node) {



//}


