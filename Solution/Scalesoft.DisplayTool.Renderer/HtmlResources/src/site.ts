import "./main.scss";
import "./collapseSectionButtonFuncs"
import {updateCollapsibleUI} from "./collapseSectionButtonFuncs";
import {expandParentCollapsers} from "./referenceLinkFunctions";
import {initModals} from "./modalFunctions";
import {arrow, computePosition, flip, offset, shift} from "@floating-ui/dom";
import {dtRootElement} from "./rootElementProvider";

updateCollapsibleUI();
expandParentCollapsers();

const noCardContentClass = "no-content";
const cardClass = "card";
const collapserCheckboxClass = "collapse-checkbox";
const cardBodyClass = "card-body";
const cardFooterClass = "card-footer";

const initTooltips = () => {
    const tooltipWrappers = dtRootElement.querySelectorAll<HTMLElement>(".tooltip-wrapper");
    tooltipWrappers.forEach((wrapperElement: HTMLElement): void => {
        const tooltipElement = wrapperElement?.querySelector<HTMLElement>(".tooltip");
        const arrowElement = wrapperElement?.querySelector<HTMLElement>(".arrow");
        if (!tooltipElement || !arrowElement) {
            return;
        }

        //removing classes that is using default tooltip without javascript
        tooltipElement.classList.remove("tooltip-top", "tooltip-bottom");
        const updateTooltip = (): void => {
            computePosition(wrapperElement, tooltipElement, {
                placement: "top",
                middleware: [
                    offset(5),
                    flip(),
                    shift({padding: 5}),
                    arrow({element: arrowElement}),
                ],
            })
                .then(({x, y, placement, middlewareData}): void => {
                    Object.assign(tooltipElement.style, {
                        left: `${x}px`,
                        top: `${y}px`,
                    });
                    const {x: arrowX, y: arrowY} = middlewareData.arrow ?? {};
                    const staticSide = {
                        top: "bottom",
                        right: "left",
                        bottom: "top",
                        left: "right",
                    }[placement.split("-")[0]] ?? "";
                    Object.assign(arrowElement.style, {
                        left: arrowX != null ? `${arrowX}px` : "",
                        top: arrowY != null ? `${arrowY}px` : "",
                        [staticSide]: "-4px",
                    });
                });
        }

        const showTooltip = (tooltipElement: HTMLElement): void => {
            tooltipElement.style.display = "block";
            updateTooltip();
            dtRootElement.appendChild(tooltipElement);
        }

        const hideTooltip = (tooltipElement: HTMLElement): void => {
            tooltipElement.style.display = "none";
            wrapperElement.appendChild(tooltipElement);
        }

        wrapperElement.addEventListener("mouseenter", () => showTooltip(tooltipElement));
        wrapperElement.addEventListener("mouseleave", () => hideTooltip(tooltipElement));
        wrapperElement.addEventListener("focus", () => showTooltip(tooltipElement));
        wrapperElement.addEventListener("blur", () => hideTooltip(tooltipElement));
        updateTooltip();
    })
}

const collapseAllButton = dtRootElement.querySelector<HTMLButtonElement>(".collapse-all-btn");
if (collapseAllButton) {
    collapseAllButton.hidden = false;
}

const isElementEmpty = (el?: Node | null): boolean => {
    if (!el) {
        return true;
    }
    if (el instanceof HTMLElement && el.classList.contains(noCardContentClass)) {
        return true;
    }
    if (!el.childNodes.length) {
        if (el.nodeType === Node.TEXT_NODE) {
            return !nodeHasText(el);
        }
        if (el.nodeType === Node.ELEMENT_NODE) {
            return elementHasVisualRepresentationWithoutChildren(el as HTMLElement);
        }

        return false;
    } else {
        const noChildHasContent = Array.from(el.childNodes).map(nodeHasContent).every(childHasContent => !childHasContent);

        return noChildHasContent;
    }
}

const nodeHasText = (node: Node): boolean => {
    let hasText = false;
    if (node.textContent?.trim()) {
        hasText = true;
    }
    return hasText;
}

const elementHasVisualRepresentationWithoutChildren = (el: HTMLElement): boolean => {
    const tagName = el.tagName;
    switch (tagName) {
        case "IMG":
        case "SVG":
        case "VIDEO":
        case "CANVAS":
        case "EMBED":
        case "OBJECT":
            return true;
        default:
            return false;
    }
}

const nodeHasContent = (node: Node): boolean => {
    if (node.nodeType === Node.TEXT_NODE) {
        return nodeHasText(node);
    } else if (node.nodeType === Node.ELEMENT_NODE) {
        const hasVisualRepresentationByItself = elementHasVisualRepresentationWithoutChildren(node as HTMLElement);
        if (hasVisualRepresentationByItself) {
            return true;
        }
        return !isElementEmpty((node as HTMLElement).shadowRoot ?? node);
    }

    return true;
};

const isInvisibleOrHasNoContent = (el: HTMLElement | null, collapserCheckbox: HTMLInputElement | null): boolean => {
    const collapsed = (collapserCheckbox && !collapserCheckbox.checked);
    // if an element is in a collapser, checking for visibility is insufficient, as it could simply be collapsed, so skip visibility check if collapser is collapsed
    return collapsed ? false : !el?.checkVisibility() || (isElementEmpty(el) && isElementEmpty(el.shadowRoot));
};

const debounceCardChecks = (cards: NodeListOf<HTMLDivElement>) => {
    debounce(() => checkCardsHaveContent(cards), 250, false);
}

const checkCardsHaveContent = (cards: NodeListOf<HTMLDivElement>, parent: HTMLDivElement | null = null): void => {
    if (cards.length === 0) {
        return;
    }

    let cardParents: HTMLDivElement[] = [];
    cards.forEach((card: HTMLDivElement) => {
        const parentCard = card.parentElement?.closest<HTMLDivElement>(`.${cardClass}`);
        if (parentCard === null || parentCard === parent) {
            cardParents.push(card);
        }
    });

    cardParents.forEach((card: HTMLDivElement) => {
        card.classList.remove(noCardContentClass);
        const children = card.querySelectorAll<HTMLDivElement>(`.${cardClass}`);
        checkCardsHaveContent(children, card);
        let hasContent = true;
        const collapserCheckbox = card.querySelector<HTMLInputElement>(`.${collapserCheckboxClass}`);
        const body = card.querySelector<HTMLElement>(`.${cardBodyClass}`);
        const footer = card.querySelector<HTMLElement>(`.${cardFooterClass}`);
        if (!body && !footer) {
            hasContent = false;
        }
        if (isInvisibleOrHasNoContent(body, collapserCheckbox) && isInvisibleOrHasNoContent(footer, collapserCheckbox)) {
            hasContent = false;
        }
        if (!hasContent) {
            card.classList.add(noCardContentClass);
        }
    });
};


const checkCardParentVisibility = (cards: NodeListOf<HTMLDivElement>, cb: (cards: NodeListOf<HTMLDivElement>) => void): void => {
    // collapsible content itself can be collapsed by its parent, so we can't rely on checking only card-body visibility
    let cardVisibility = new Map<HTMLDivElement, boolean>();
    cards.forEach((card: HTMLDivElement) => {
        cardVisibility.set(card, card.checkVisibility());
    });
    // ideally, we would want to trigger on visibility change event, but since it doesn't exist, trigger on user interactions
    ["click", "keypress"].forEach(ev => document.addEventListener(ev, () => {
        let anyCardParentVisibilityChanged = false;
        cards.forEach((card: HTMLDivElement) => {
            const cardIsVisible = card.checkVisibility();
            if (cardVisibility.get(card) !== cardIsVisible) {
                anyCardParentVisibilityChanged = true;
                cardVisibility.set(card, cardIsVisible);
            }
        });
        if (anyCardParentVisibilityChanged) {
            anyCardParentVisibilityChanged = false;
            cb(cards);
        }
    }));
}

const initCardContentChecks = (): void => {
    const cards = dtRootElement.querySelectorAll<HTMLDivElement>(`.${cardClass}`);
    debounceCardChecks(cards);
    if (window.matchMedia) {
        const mediaQueryList = window.matchMedia("print");
        mediaQueryList.addEventListener("change", () => {
            debounceCardChecks(cards);
        });
    }
    const detailToggleCheckbox = dtRootElement.querySelector<HTMLInputElement>("#detail-toggle");
    if (detailToggleCheckbox) {
        detailToggleCheckbox.addEventListener("change", () => debounceCardChecks(cards));
    }
    checkCardParentVisibility(cards, debounceCardChecks);
};

dtRootElement.querySelectorAll('.modal').forEach((modal: Element) => {
    modal.addEventListener("click", (e) => {
        const target = e.target as HTMLElement;
        if (!target.closest("button")) {
            e.preventDefault()
        }
    })
});

const debounce = (func: () => void, wait: number, immediate: boolean): () => void => {
    let timeout: number | null = null;
    return function () {
        const later = function () {
            timeout = null;
            if (!immediate) func();
        };
        const callNow = immediate && !timeout;
        if (timeout) {
            window.clearTimeout(timeout);
        }
        timeout = window.setTimeout(later, wait);
        if (callNow) {
            func();
        }
    }
};

document.addEventListener("DOMContentLoaded", () => {
    initTooltips();
    initModals();
    initCardContentChecks();
}, {once: true});