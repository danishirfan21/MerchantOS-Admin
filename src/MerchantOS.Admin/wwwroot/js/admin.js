/* Progressive enhancement: Razor GET forms remain usable without JavaScript. */
(function ($) {
  'use strict';
  var $form = $('#catalog-filters');
  if (!$form.length) return;
  var pending, sequence = 0, timer;
  function load() {
    var current = ++sequence;
    if (pending) pending.abort();
    $('#filter-status').text('Loading products…');
    $('#product-rows').attr('aria-busy', 'true');
    pending = $.ajax({url: $form.data('endpoint'), data: $form.serialize(), dataType: 'json'});
    pending.done(function (rows) {
      if (current !== sequence) return;
      var normalized = rows.map(function (row) {
        return Object.assign({}, row, {detailUrl: '/Admin/Product/' + encodeURIComponent(row.id)});
      });
      $('#product-rows').html(Handlebars.templates.products({products: normalized}));
      $('#filter-status').text(rows.length + ' products found.');
      $('.record-count').text(rows.length + ' results');
      history.replaceState(null, '', location.pathname + '?' + $form.serialize());
    }).fail(function (_, status) {
      if (current !== sequence || status === 'abort') return;
      $('#filter-status').text('Unable to refresh products. Previous results remain visible. Apply filters to retry.');
    }).always(function () {
      if (current === sequence) $('#product-rows').removeAttr('aria-busy');
    });
  }
  $form.on('submit', function (event) { event.preventDefault(); clearTimeout(timer); load(); });
  $form.find('select').on('change', load);
  $form.find('input').on('input', function () { clearTimeout(timer); timer = setTimeout(load, 300); });
})(jQuery);
