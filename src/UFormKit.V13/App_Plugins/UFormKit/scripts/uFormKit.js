function makeExclusive(element) {
    var isChecked = element.checked;

    if (isChecked) {
        document.getElementsByName(element.getAttribute("name")).forEach(el => el.checked = false);;
        element.checked = true;
    }
}

function characterCount(object, down) {
    var maxLength = object.getAttribute("maxLength");
        var stringLength = object.value.length;
        var remainCharacters = (maxLength - stringLength);

        if (down) {
            document.querySelector('span[data-target-name="' + object.getAttribute("name") + '"]').innerHTML = remainCharacters;
        } else {
            document.querySelector('span[data-target-name="' + object.getAttribute("name") + '"]').innerHTML = stringLength;
        }
        
    }

