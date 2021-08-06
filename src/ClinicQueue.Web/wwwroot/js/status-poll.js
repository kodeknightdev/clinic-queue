// Keeps the patient's place in the queue up to date on the status page
// without them having to refresh.
(function () {
  'use strict';

  var root = document.getElementById('queue-status');
  if (!root || !window.fetch) {
    return;
  }

  var url = root.getAttribute('data-poll-url');
  var interval = 15000;
  var maxDelay = 120000;
  var failures = 0;
  var timer = null;
  var finished = false;

  function set(name, value) {
    var el = root.querySelector('[data-field="' + name + '"]');
    if (el) {
      el.textContent = value;
    }
  }

  function describeEstimate(minutes) {
    if (minutes === null || minutes === undefined) {
      return '–';
    }
    return minutes < 1 ? 'any minute now' : 'about ' + minutes + ' min';
  }

  function render(data) {
    set('status', data.statusLabel);
    set('position', data.position !== null ? data.position : '–');
    set('estimate', describeEstimate(data.estimatedMinutes));

    var wasCalled = root.classList.contains('is-called');
    var isCalled = data.status === 'Called';
    root.classList.toggle('is-called', isCalled);
    if (isCalled && !wasCalled && navigator.vibrate) {
      navigator.vibrate([200, 100, 200]);
    }
    finished = data.isFinal;
  }

  function schedule(delay) {
    window.clearTimeout(timer);
    timer = finished ? null : window.setTimeout(poll, delay);
  }

  function poll() {
    timer = null;
    fetch(url, { cache: 'no-store', headers: { 'Accept': 'application/json' } })
      .then(function (res) {
        if (!res.ok) {
          throw new Error('HTTP ' + res.status);
        }
        return res.json();
      })
      .then(function (data) {
        failures = 0;
        render(data);
        schedule(interval);
      })
      .catch(function () {
        failures++;
        schedule(Math.min(interval * Math.pow(2, failures), maxDelay));
      });
  }

  document.addEventListener('visibilitychange', function () {
    if (document.hidden) {
      window.clearTimeout(timer);
      timer = null;
    } else if (!timer && !finished) {
      poll();
    }
  });

  schedule(interval);
})();
