let dtRoot: Document | ShadowRoot = document.querySelector<HTMLElement>('.dt-shadow-root-host')?.shadowRoot ?? document;
export let dtRootElement : HTMLElement = dtRoot.querySelector<HTMLElement>(`.display-tool-body`)
    ?? (() => {throw new Error("Display tool body element not found.")})();
