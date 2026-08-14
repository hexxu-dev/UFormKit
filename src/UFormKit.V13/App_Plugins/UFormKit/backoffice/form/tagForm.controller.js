(function () {
    'use strict';

    function customOverlayController($scope, $filter) {


        var vm = this;
        vm.formTag = {
            type: $scope.model.tag.type,
            numType:"",
            required: false,
            name: "",
            id: "",
            class: "",
            values: "",
            placeholder: false,
            value: "",
            minNum: null,
            maxNum: null,
            minDate: null,
            maxDate: null,
            options: null,
            multiple: false,
            includeBlank: false,
            labelFirst: false,
            useLabelElement: true,
            exclusive: false,
            condition: "",
            optional: true,
            limit: null,
            filetypes: "",
            labelSub: "",
            size: 40,
            cols: 40,
            rows: 10,
            query: "",
            valueHid:""
        };

        vm.formTag.name = $scope.model.tag.type + "-" + Math.floor(Math.random() * 1000);
        populateValue();

        vm.populateValue = populateValue;

        function populateValue() {

            var tag = $scope.model.tag.type;

            if (vm.formTag.numType != "") {
                tag = vm.formTag.numType;
            }
            var value = "[" + tag;

            if (vm.formTag.required) {
                value += "*";
            }

            if (tag != "submit") {
                value += " " + vm.formTag.name;
            }

            if (vm.formTag.minNum != null) {
                value += " min:" + vm.formTag.minNum;
            }

            if (vm.formTag.maxNum != null) {
                value += " max:" + vm.formTag.maxNum;
            }

            if (vm.formTag.minDate != null) {
                value += " min:" + $filter('date')(vm.formTag.minDate, 'yyyy-MM-dd');
            }

            if (vm.formTag.maxDate != null) {
                value += " max:" + $filter('date')(vm.formTag.maxDate, 'yyyy-MM-dd');
            }

            if (vm.formTag.limit != null) {
                value += " limit:" + vm.formTag.limit;
            }

            if (vm.formTag.filetypes != "") {
                value += " filetypes:" + vm.formTag.filetypes.replace(/[^0-9a-z.]/gi, '|');
            }

            if (vm.formTag.id != "") {
                value += " id:" + vm.formTag.id;
            }

            if (vm.formTag.class != "") {
                var classArray = vm.formTag.class.split(" ");
                $.each(classArray, function (i, item) {
                    if (item) {
                        value += " class:" + item;
                    }
                });                         
            }

            if (vm.formTag.size != "" && ((tag == "text" || tag == "email" || tag=="url" || tag=="tel"))) {
                value += " size:" + vm.formTag.size;
            }

            if (vm.formTag.rows != "" && tag=="textarea") {
                value += " rows:" + vm.formTag.rows;
            }

            if (vm.formTag.cols != "" && tag== "textarea") {
                value += " cols:" + vm.formTag.cols;
            }

            if (vm.formTag.labelSub != "") {
                value += " \"" + vm.formTag.labelSub+"\"";
            }

            if (vm.formTag.placeholder) {
                value += " placeholder";
            }

            if (vm.formTag.values != "") {
                value += " \"" + vm.formTag.values + "\"";
            }

            if (vm.formTag.multiple) {
                value += " multiple ";
            }

            if (vm.formTag.includeBlank) {
                value += " include_blank";
            }

            if (vm.formTag.labelFirst) {
                value += " label_first  ";
            }

            if (vm.formTag.useLabelElement && (tag == "radio" || tag =="checkbox")) {
                value += " use_label_element ";
            }

            if (vm.formTag.exclusive) {
                value += " exclusive ";
            }

            if (tag == "radio") {
                value += " default:1"
            }

            if (vm.formTag.optional && tag == "acceptance") {
                value += " optional"
            }

            if (vm.formTag.options != null) {
                var lines = vm.formTag.options.split(/\n/);
                $.each(lines, function (i, line) {
                    if (line) {
                        value += " \"" + line + "\"";
                    } 
                });                         
            }

            if (vm.formTag.query != "") {
                value += " \"query:" + vm.formTag.query + "\"";
            }

            value += "]";

            if (vm.formTag.condition != "") {
                value += " " + vm.formTag.condition + " [/acceptance]";
            }
            vm.formTag.value = value;
            $scope.model.tagValue = value;
        }
    }

    angular.module('umbraco')
        .controller('tagFomController', customOverlayController);
})();