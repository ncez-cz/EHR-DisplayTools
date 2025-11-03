import {updateCollapsibleUI} from "./collapseSectionButtonFuncs";

/**
 * Checks if an element is visible in the viewport
 */
function isElementInView(element: HTMLElement): boolean {
    const rect = element.getBoundingClientRect();
    return (
        rect.bottom > 0 &&
        rect.right > 0 &&
        rect.top < window.innerHeight &&
        rect.left < window.innerWidth
    );
}

function temporarilyDisableTransition(el: HTMLElement, doChange: () => void) {
    const original = el.style.transition;
    el.style.transition = "none";
    doChange();
    requestAnimationFrame(() => {
        el.style.transition = original;
    });
}

function clearActiveTargets() {
    const previous = document.querySelectorAll<HTMLElement>(".activeTarget");
    if (previous) {
        previous.forEach((el) => {
            el.classList.remove("activeTarget");
        });
    }
}

function expandToElement(target: HTMLElement): void {
    requestAnimationFrame(() => {
        target.classList.add("activeTarget");
    });

    let current: HTMLElement | null = target.parentElement;
    let i: number = 0;
    while (current) {
        const checkbox = current.querySelector<HTMLInputElement>(":scope > input[type='checkbox']");
        const wrapper = current.querySelector<HTMLElement>(":scope > .collapsible-content-wrapper");

        if (checkbox && !checkbox.checked) {
            const applyCheck = () => {
                checkbox.checked = true;
            };
            wrapper && !isElementInView(wrapper)
                ? temporarilyDisableTransition(wrapper, applyCheck)
                : applyCheck();
        }

        if (current instanceof HTMLTableSectionElement && current.tagName === "TBODY") {
            const nextTbody = current.nextElementSibling;
            const isMultiTable = nextTbody instanceof HTMLTableSectionElement && nextTbody.tagName === "TBODY";

            const firstCheckbox = current.querySelector<HTMLInputElement>(":scope > .visible-row .collapse-toggler-checkbox");
            const collapsibleRowCheckbox = current.querySelector<HTMLInputElement>(":scope > .collapse-toggler-row .collapse-toggler-checkbox");

            let checkbox = firstCheckbox;
            if (!isMultiTable) {
                checkbox = collapsibleRowCheckbox;
            }

            if (checkbox && !checkbox.checked) {
                if (i != 0) {
                    checkbox.checked = true;
                }
            }
        }


        if (current.classList.contains("section")) {
            const parentSection = current.parentElement?.closest<HTMLElement>(".section");
            current = parentSection ?? null;
        } else {
            current = current.parentElement;
        }

        i++;
    }
    
    updateCollapsibleUI();
    target.scrollIntoView({block: "center"});
}

/**
 * Expands all parent collapsers of the given target and waits for transitions on visible ones.
 */
export function expandParentCollapsers(): void {
    clearActiveTargets();
    const target = document.querySelector<HTMLElement>("*:target");
    if (!target) {
        return;
    }

    if (!target.checkVisibility()) {
        // Redirect to a non-hidden element containing the same resource if possible 
        const id = target.id;
        const elementsWithId = document.querySelectorAll<HTMLElement>(`*[data-id="${id}"]`);
        const visibleTarget = Array.from(elementsWithId).find(el => el.checkVisibility());
        if (visibleTarget) {
            expandToElement(visibleTarget);
            return;
        }
    }

    expandToElement(target);
}

(window as Window).expandParentCollapsers = expandParentCollapsers;

window.addEventListener("hashchange", () => {
    expandParentCollapsers();
});

document.querySelectorAll<HTMLAnchorElement>("a[href^='#']").forEach((link) => {
    link.addEventListener("click", e => {
        //This is to trigger the hashchange event even when the same hash is already set - which wouldn't normally trigger it
        if (window.location.hash === link.hash) {
            e.preventDefault();
            window.dispatchEvent(new HashChangeEvent("hashchange"));
        }
    })
});