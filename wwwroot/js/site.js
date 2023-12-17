// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function add_input_fields(id) {
	document.getElementById(id).insertAdjacentHTML('beforeend', '<input type="text" name="new_property_value" id="new_property_value" value=""><br>');
};

function htmlDecode(input) {
	var doc = new DOMParser().parseFromString(input, "text/html");
	return doc.documentElement.textContent;
}