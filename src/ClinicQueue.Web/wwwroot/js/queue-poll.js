// Live queue panel on the staff dashboard.
// Polls the dashboard's Snapshot handler and updates the counts and waiting list in place,
// so the desk can see new check-ins without reloading the page.
(function () {
  'use strict';

  var panel = document.getElementById('live-queue');
  if (!panel || !window.fetch) {
    return;
  }

  var url = panel.getAttribute('data-snapshot-url');
  var interval = 10000;
  var maxDelay = 60000;
  var failures = 0;
  var timer = null;

  function field(name) {
    return panel.querySelector('[data-field="' + name + '"]');
  }

  function setCount(name, value) {
    var el = panel.querySelector('[data-count="' + name + '"]');
    if (el) {
      el.textContent = value;
    }
  }

  function renderList(items) {
    var list = field('list');
    while (list.firstChild) {
      list.removeChild(list.firstChild);
    }

    if (!items.length) {
      var empty = document.createElement('li');
      empty.className = 'live-empty';
      empty.textContent = 'Nobody waiting.';
      list.appendChild(empty);
      return;
    }

    items.forEach(function (item) {
      var li = document.createElement('li');
      if (item.isPriority) {
        li.className = 'is-priority';
      }

      var ticket = document.createElement('span');
      ticket.className = 'live-ticket';
      ticket.textContent = '#' + item.ticket;

      var name = document.createElement('span');
      name.className = 'live-name';
      name.textContent = item.name;

      var waited = document.createElement('span');
      waited.className = 'live-waited';
      waited.textContent = item.waitedMinutes + ' min';

      li.appendChild(ticket);
      li.appendChild(name);
      li.appendChild(waited);
      list.appendChild(li);
    });
  }

  function render(snapshot) {
    setCount('waitingCount', snapshot.waitingCount);
    setCount('calledCount', snapshot.calledCount);
    setCount('inProgressCount', snapshot.inProgressCount);
    renderList(snapshot.waiting);
    field('updated').textContent = 'Updated ' + new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  function schedule(delay) {
    window.clearTimeout(timer);
    timer = window.setTimeout(poll, delay);
  }

  function poll() {
    timer = null;
    fetch(url, {
      credentials: 'same-origin',
      cache: 'no-store',
      headers: { 'Accept': 'application/json' }
    })
      .then(function (res) {
        if (!res.ok) {
          throw new Error('HTTP ' + res.status);
        }
        return res.json();
      })
      .then(function (snapshot) {
        failures = 0;
        render(snapshot);
        schedule(interval);
      })
      .catch(function () {
        // Server restarting / wifi blip: keep trying, but back off.
        failures++;
        schedule(Math.min(interval * Math.pow(2, failures), maxDelay));
      });
  }

  // No point polling while the tab is in the background.
  document.addEventListener('visibilitychange', function () {
    if (document.hidden) {
      window.clearTimeout(timer);
      timer = null;
    } else if (!timer) {
      poll();
    }
  });

  poll();
})();
