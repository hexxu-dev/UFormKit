function makeExclusive(element) {
    var isChecked = element.checked;

    if (isChecked) {
        document.getElementsByName(element.getAttribute("name")).forEach(el => el.checked = false);;
        element.checked = true;
    }
}

