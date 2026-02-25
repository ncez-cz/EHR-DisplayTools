import {dtRootElement} from "./rootElementProvider";

/**
 * Gets the collapse/expand text from the data attributes
 * @returns {Object} Object containing collapse and expand text
 */
function getCollapseExpandText(): { collapseText: string; expandText: string } {
    const dataDiv = dtRootElement.querySelector<HTMLDivElement>('div[data-collapse-label]');
    return {
        collapseText: dataDiv?.dataset.collapseLabel || 'Sbalit všechny sekce',
        expandText: dataDiv?.dataset.expandLabel || 'Rozbalit všechny sekce'
    };
}

/**
 * Toggles all collapsible sections and updates button text accordingly
 */
export function expandOrCollapseAllSections(): void {
    // Get all collapse checkboxes
    const checkboxes = dtRootElement.querySelectorAll<HTMLInputElement>('div.section > input.collapse-checkbox');

    // Determine the current state (if any are checked, we'll collapse all)
    const shouldCollapse = Array.from(checkboxes).some(cb => cb.checked);

    // Toggle all checkboxes to the new state
    checkboxes.forEach(cb => {
        cb.checked = !shouldCollapse;
    });

    // Get text from data attributes
    const { collapseText, expandText } = getCollapseExpandText();

    // Update button text based on the NEW state (after toggling)
    updateButtonText(!shouldCollapse ? collapseText : expandText);
}

/**
 * Updates only the text on expand/collapse buttons without changing checkbox states
 * @param newText - The text to display on the buttons
 */
function updateButtonText(newText: string): void {
    const buttons = dtRootElement.querySelectorAll<HTMLLabelElement>('label[onclick*="expandOrCollapseAllSections"]');

    buttons.forEach(button => {
        // Update text content
        const span = button.querySelector<HTMLSpanElement>("span");
        if (span) {
            span.textContent = newText;
        }
    });
}

/**
 * Updates button text based on the current state of checkboxes
 */
export function updateCollapsibleUI(): void {
    // Get all checkboxes
    const checkboxes = dtRootElement.querySelectorAll<HTMLInputElement>('div.section > input.collapse-checkbox');

    // Check if any checkboxes are checked
    const anyChecked = Array.from(checkboxes).some(cb => cb.checked);

    // Get text from data attributes
    const { collapseText, expandText } = getCollapseExpandText();

    // Update button text based on current state
    updateButtonText(anyChecked ? collapseText : expandText);

    // Update control checkboxes if needed
    const buttons = dtRootElement.querySelectorAll<HTMLLabelElement>('label[onclick*="expandOrCollapseAllSections"]');
    buttons.forEach(button => {
        // Update associated checkbox
        const checkboxId = button.htmlFor;
        if (checkboxId) {
            const checkbox = dtRootElement.querySelector(`#checkboxId`) as HTMLInputElement | null;
            if (checkbox) {
                checkbox.checked = anyChecked;
            }
        }
    });
}

(window as Window).expandOrCollapseAllSections = expandOrCollapseAllSections;
(window as Window).updateName = updateCollapsibleUI;