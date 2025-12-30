let observer = null;

export function observeElement(element, dotNetRef) {
    if (observer) {
        observer.disconnect();
    }

    if (!element) return;

    observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                dotNetRef.invokeMethodAsync('OnIntersecting');
            }
        });
    }, {
        root: null,
        rootMargin: '100px',
        threshold: 0.1
    });

    observer.observe(element);
}

export function disconnect() {
    if (observer) {
        observer.disconnect();
        observer = null;
    }
}

