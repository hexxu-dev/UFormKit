function formController($scope, validationMessageService, overlayService) {
    // macro parameter editor doesn't contains a config object,
    // so we create a new one to hold any properties
    if (!$scope.model.config) {
        $scope.model.config = {};
    }

    $scope.tags = [
        { name: "text", type:"text", view:"tagFormDefault"},
        { name: "email", type: "email", view: "tagFormDefault" },
        { name: "URL", type: "url", view: "tagFormDefault" },
        { name: "tel", type: "tel", view: "tagFormDefault" },
        { name: "number", type: "number", view: "tagFormDefault" },
        { name: "date", type: "date", view: "tagFormDefault" },
        { name: "text area", type: "textarea", view: "tagFormDefault" },
        { name: "drop-down menu", type: "select", view: "tagFormList" },
        { name: "checkboxes", type: "checkbox", view: "tagFormList" },
        { name: "radio buttons", type: "radio", view: "tagFormList" },
        { name: "acceptance", type: "acceptance", view: "tagFormAcceptance" },
        { name: "file", type: "file", view: "tagFormFile" },
        { name: "submit", type: "submit", view: "tagFormSubmit" },
        { name: "hidden", type: "hidden", view: "tagFormHidden" },
    ]

    $scope.tagValue = "";

    $scope.openForm = function (tag) 
    {
            overlayService.open({
            view: '/App_Plugins/UFormKit/backoffice/form/' + tag.view + '.html',
            title: "Form-tag Generator: " + tag.name,
            submitButtonLabel: "Insert tag",
            tag: tag,
            tagValue: $scope.tagValue,
            submit: (model) => {
                setTextToCurrentPos(model.tagValue);
                overlayService.close();
                             
            },
            close: () => overlayService.close()
        });
    }

    function setTextToCurrentPos(text) {
        var id = $scope.model.alias;
        var curPos = document.getElementById(id).selectionStart;
        let x = document.getElementById(id).value;
        let text_to_insert = text;
        document.getElementById(id).value = 
            x.slice(0, curPos) + text_to_insert + x.slice(curPos);

        $scope.model.value = document.getElementById(id).value;
    }
}
angular.module('umbraco').controller("Umbraco.PropertyEditors.formController", formController);