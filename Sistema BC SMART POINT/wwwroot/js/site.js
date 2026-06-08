(function () {
    'use strict';

    var STORAGE_KEY = 'bc-a11y-mode';
    var MODES = ['modo-claro', 'modo-oscuro', 'alto-contraste'];

    function applyMode(mode) {
        MODES.forEach(function (m) { document.body.classList.remove(m); });
        if (mode && mode !== 'modo-claro') {
            document.body.classList.add(mode);
        }
        try { localStorage.setItem(STORAGE_KEY, mode || 'modo-claro'); } catch (e) { }
    }

    function getStoredMode() {
        try { return localStorage.getItem(STORAGE_KEY) || 'modo-claro'; } catch (e) { return 'modo-claro'; }
    }

    function updateActiveItem(mode) {
        var menu = document.getElementById('bcA11yMenu');
        if (!menu) return;
        menu.querySelectorAll('.bc-a11y-item').forEach(function (item) {
            item.classList.toggle('active', item.getAttribute('data-mode') === mode);
        });
    }

    function initA11y() {
        var btn = document.getElementById('bcA11yBtn');
        var menu = document.getElementById('bcA11yMenu');
        if (!btn || !menu) return;

        applyMode(getStoredMode());
        updateActiveItem(getStoredMode());

        btn.addEventListener('click', function (e) {
            e.stopPropagation();
            var isOpen = menu.classList.toggle('open');
            btn.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
        });

        document.addEventListener('click', function (e) {
            if (!menu.contains(e.target) && e.target !== btn) {
                menu.classList.remove('open');
                btn.setAttribute('aria-expanded', 'false');
            }
        });

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                menu.classList.remove('open');
                btn.setAttribute('aria-expanded', 'false');
                btn.focus();
            }
        });

        menu.querySelectorAll('.bc-a11y-item').forEach(function (item) {
            item.addEventListener('click', function () {
                var mode = item.getAttribute('data-mode');
                applyMode(mode);
                updateActiveItem(mode);
                menu.classList.remove('open');
                btn.setAttribute('aria-expanded', 'false');
            });
        });
    }

    function bcSubscribe(btn) {
        var form = btn.closest('.bc-newsletter__form');
        var input = form ? form.querySelector('.bc-newsletter__input') : null;
        if (!input) return;

        var email = input.value.trim();
        if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
            input.style.borderColor = '#ef4444';
            input.focus();
            setTimeout(function () { input.style.borderColor = ''; }, 2000);
            return;
        }

        btn.textContent = '¡Suscrito!';
        btn.disabled = true;
        btn.style.background = '#22c55e';
        input.value = '';
        input.disabled = true;

        setTimeout(function () {
            btn.textContent = 'Suscribirme';
            btn.disabled = false;
            btn.style.background = '';
            input.disabled = false;
        }, 3500);
    }

    function initNewsletter() {
        document.querySelectorAll('.bc-newsletter__submit').forEach(function (btn) {
            btn.addEventListener('click', function () {
                bcSubscribe(btn);
            });
        });
    }

    document.addEventListener('click', function (e) {
        var favBtn = e.target.closest('.bc-card__fav');
        if (!favBtn) return;
        var svg = favBtn.querySelector('svg');
        if (!svg) return;
        var active = favBtn.classList.toggle('bc-card__fav--active');
        svg.style.fill = active ? '#ef4444' : 'none';
        svg.style.stroke = active ? '#ef4444' : '#bbb';
    });

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            initA11y();
            initNewsletter();
        });
    } else {
        initA11y();
        initNewsletter();
    }
}());
