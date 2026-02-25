import {updateCollapsibleUI} from "./collapseSectionButtonFuncs";
import {dtRootElement} from "./rootElementProvider";

function clearActiveTargets() {
    const previous = dtRootElement.querySelectorAll<HTMLElement>(".activeTarget");
    if (previous) {
        previous.forEach((el) => {
            el.classList.remove("activeTarget");
        });
    }
}

function waitForTransitionEnd(el: HTMLElement): Promise<void> {
    return new Promise(resolve => {
        const duration = parseFloat(getComputedStyle(el).transitionDuration);
        if (!duration || duration === 0) {
            resolve();
            return;
        }

        const onEnd = (e: TransitionEvent) => {
            if (e.target === el) {
                el.removeEventListener("transitionend", onEnd);
                resolve();
            }
        };
        el.addEventListener("transitionend", onEnd);
    });
}

async function expandToElement(target: HTMLElement): Promise<void> {
    requestAnimationFrame(() => target.classList.add("activeTarget"));

    let current: HTMLElement | null = target.parentElement;
    let i = 0;
    const toWait: Promise<void>[] = [];

    while (current) {
        const checkbox = current.querySelector<HTMLInputElement>(":scope > input[type='checkbox']");
        const wrapper = current.querySelector<HTMLElement>(":scope > .collapsible-content-wrapper");

        if (checkbox && !checkbox.checked) {
            checkbox.checked = true;
            if (wrapper) {
                toWait.push(waitForTransitionEnd(wrapper));
            }
        }

        if (current instanceof HTMLTableSectionElement && current.tagName === "TBODY") {
            const nextTbody = current.nextElementSibling;
            const isMultiTable = nextTbody instanceof HTMLTableSectionElement && nextTbody.tagName === "TBODY";

            const firstCheckbox = current.querySelector<HTMLInputElement>(":scope > .visible-row .collapse-toggler-checkbox");
            const collapsibleRowCheckbox = current.querySelector<HTMLInputElement>(":scope > .collapse-toggler-row .collapse-toggler-checkbox");

            let tableCheckbox = firstCheckbox;
            if (!isMultiTable) {
                tableCheckbox = collapsibleRowCheckbox;
            }

            if (tableCheckbox && !tableCheckbox.checked && i !== 0) {
                tableCheckbox.checked = true;
            }
        }

        current = current.classList.contains("section")
            ? current.parentElement?.closest<HTMLElement>(".section") ?? null
            : current.parentElement;

        i++;
    }

    updateCollapsibleUI();
    await Promise.allSettled(toWait);
    target.scrollIntoView({block: "center"});
}

function getTargetFromUrlHash(parent: HTMLElement): HTMLElement | null {
    const hash = window.location.hash;
    if (!hash.startsWith("#")) { // selected element must be an id
        return null;
    }
    const id = hash.substring(1);
    const escapedId = CSS.escape(id);

    return parent.querySelector<HTMLElement>(`#${escapedId}`);
}

/**
 * Expands all parent collapsers of the given target and waits for transitions on visible ones.
 */
export async function expandParentCollapsers(): Promise<void> {
    clearActiveTargets();
    const target = getTargetFromUrlHash(dtRootElement);
    if (!target) {
        return;
    }

    if (!target.checkVisibility()) {
        // Redirect to a non-hidden element containing the same resource if possible 
        const id = target.id;
        const elementsWithId = dtRootElement.querySelectorAll<HTMLElement>(`*[data-id="${id}"]`);
        const visibleTarget = Array.from(elementsWithId).find(el => el.checkVisibility());
        if (visibleTarget) {
            await expandToElement(visibleTarget)
            return;
        }
    }

    await expandToElement(target);
}

(window as Window).expandParentCollapsers = expandParentCollapsers;

window.addEventListener("hashchange", async () => {
    await expandParentCollapsers();
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