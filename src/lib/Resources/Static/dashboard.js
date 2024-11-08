(() => {

    document.addEventListener('DOMContentLoaded', () => {
        document.getElementById('exportButton').addEventListener('click', () => {
            const jsonBlob = new Blob([JSON.stringify(window.Export)], { type: 'application/json' }); 
            const url = URL.createObjectURL(jsonBlob);

            const tempLink = document.createElement('a');
            tempLink.href = url;

            var fileName = 'assemblies.json';
            if (window.Export.length === 1) {
                fileName = window.Export[0].Name + '.json';
            }

            tempLink.download = fileName;

            document.body.appendChild(tempLink);
            tempLink.click();
            document.body.removeChild(tempLink);
        });

        
        onLoad();
        
        document.getElementById('searchBar').addEventListener('input', (e) => {
            onSearch(e.target.value);
        });
        
    });
})();

function onLoad() {
    
    // setup classes for bootstrap collapsible elements
    AllControllers().forEach(controller => {
        controller.classList.add('collapse','show');

        AllReferencingElements(controller).forEach(element => {
            element.parentElement.classList.add('collapse','show');
        });
    });

    AllActions().forEach(action => {
        action.classList.add('collapse','show');
    });

}

function onSearch(newSearch) {
    const search = newSearch;
    AllControllers().forEach(controller => {
        var containsText = controller.innerText.toLowerCase().includes(search);
        var shouldHide = search.length > 0 && !containsText;

        toggleElementVisibility(controller, shouldHide, false);
        setDisplayForReferencingElements(controller, shouldHide);
    });

    // find actions we should show/hide
    AllActions().forEach(action => {
        var containsText = action.innerText.toLowerCase().includes(search);
        var shouldHide = search.length > 0 && !containsText;
        toggleElementVisibility(action, shouldHide, true);
    });
}



function AllControllers() {
    return Array.from(document.getElementsByClassName('controller'));
}

function AllActions() {
    return Array.from(document.getElementsByClassName('action'));
}

function toggleElementVisibility(element, shouldHide, fast = true) {

    var collapsing = isCollapsingOrHidden(element);
    var needsToggle = (collapsing && !shouldHide) || (!collapsing && shouldHide);
    if (needsToggle) {
        setCollapse(element, shouldHide, fast);
    }
}

function setCollapse(element, collapse, fast = true) {
    if (fast) {
        element.style.display = collapse ? 'none' : 'block';
    } else {
        var collapseInstance = bootstrap.Collapse.getOrCreateInstance(element, { toggle: false });
        if (collapse) {
            collapseInstance.hide();
        } else {
            collapseInstance.show();
        }
    }
}

function isCollapsingOrHidden(element) {
    return element.style.display === 'none' || 
        element.classList.contains('collapsing') || 
        (element.classList.contains('collapse') && !element.classList.contains('show'));
}

function AllReferencingElements(element) {
    return document.querySelectorAll(`[href="#${element.id}"]`);
}

function setDisplayForReferencingElements(element, shouldHide) {
    // find all elements with hrefs pointing to the id
    AllReferencingElements(element).forEach(element => {
        toggleElementVisibility(element.parentElement, shouldHide, false);
    });
}