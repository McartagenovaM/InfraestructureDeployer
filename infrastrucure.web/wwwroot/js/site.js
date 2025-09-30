// Bootstrap helpers for the components index page
(() => {
    const disableAnchor = (anchor) => {
        if (!anchor) {
            return;
        }
        anchor.classList.add('disabled');
        anchor.setAttribute('aria-disabled', 'true');
        anchor.setAttribute('tabindex', '-1');
    };

    const initActionButtons = () => {
        const forms = document.querySelectorAll('[data-component-action-group] form');
        forms.forEach(form => {
            form.addEventListener('submit', event => {
                const submitter = event.submitter;
                const group = form.closest('[data-component-action-group]');

                if (submitter) {
                    submitter.classList.add('loading');
                    window.setTimeout(() => {
                        submitter.disabled = true;
                    }, 0);
                }

                if (group) {
                    window.setTimeout(() => {
                        group.dataset.loading = 'true';
                        group.querySelectorAll('button').forEach(button => {
                            button.disabled = true;
                            if (button !== submitter) {
                                button.classList.remove('loading');
                            }
                        });
                        const actionLink = group.querySelector('a');
                        disableAnchor(actionLink);
                    }, 0);
                }
            });
        });
    };

    const initTooltips = () => {
        const triggers = document.querySelectorAll('[data-bs-toggle="tooltip"]');
        triggers.forEach(el => {
            if (!bootstrap.Tooltip.getInstance(el)) {
                new bootstrap.Tooltip(el, { delay: { show: 200, hide: 0 } });
            }
        });
    };

    const initToasts = () => {
        const container = document.getElementById('toast-container');
        if (!container) {
            return;
        }

        container.querySelectorAll('.toast').forEach(toastEl => {
            bootstrap.Toast.getOrCreateInstance(toastEl).show();
        });
    };

    const initConfirmDelete = () => {
        const modalEl = document.getElementById('confirmDeleteModal');
        if (!modalEl) {
            return;
        }

        const idInput = modalEl.querySelector('#confirmDeleteId');
        const nameTarget = modalEl.querySelector('[data-component-name]');
        const submitButton = modalEl.querySelector('[data-confirm-delete-submit]');
        const spinner = submitButton ? submitButton.querySelector('.spinner-border') : null;
        const icon = submitButton ? submitButton.querySelector('[data-icon]') : null;

        const resetButtonState = () => {
            if (submitButton) {
                submitButton.classList.remove('loading');
                submitButton.removeAttribute('disabled');
            }
            if (spinner) {
                spinner.classList.add('d-none');
            }
            if (icon) {
                icon.classList.remove('d-none');
            }
        };

        modalEl.addEventListener('show.bs.modal', event => {
            resetButtonState();
            const trigger = event.relatedTarget;
            const componentId = trigger?.getAttribute('data-component-id') ?? '';
            const componentName = trigger?.getAttribute('data-component-name') ?? 'this component';
            if (idInput) {
                idInput.value = componentId;
            }
            if (nameTarget) {
                nameTarget.textContent = componentName;
            }
        });

        modalEl.addEventListener('hidden.bs.modal', () => {
            if (idInput) {
                idInput.value = '';
            }
            if (nameTarget) {
                nameTarget.textContent = 'this component';
            }
            resetButtonState();
        });

        const form = modalEl.querySelector('form');
        if (form) {
            form.addEventListener('submit', event => {
                const submitter = event.submitter ?? submitButton;
                if (!submitter) {
                    return;
                }
                submitter.classList.add('loading');
                if (spinner) {
                    spinner.classList.remove('d-none');
                }
                if (icon) {
                    icon.classList.add('d-none');
                }
                window.setTimeout(() => {
                    submitter.setAttribute('disabled', 'true');
                }, 0);
            });
        }
    };

    document.addEventListener('DOMContentLoaded', () => {
        initActionButtons();
        initTooltips();
        initToasts();
        initConfirmDelete();
    });
})();
