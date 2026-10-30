/* Progressive enhancement. Server-side GET forms and record links remain the fallback. */
(function ($) {
  'use strict';
  $(document).on('keydown', function (event) { if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k' && $('#global-search').is(':visible')) { event.preventDefault(); $('#global-search').trigger('focus'); } });
  $('#sidebar').on('shown.bs.offcanvas hidden.bs.offcanvas', function (event) {
    $('.navigation-toggle').attr('aria-expanded', event.type === 'shown' ? 'true' : 'false');
  });
  $('.print-receipt').on('click', function () { window.print(); });
  $('.order-filters select').on('change', function () { this.form.requestSubmit(); });
  var drawerRequest, drawerSequence = 0, drawerTrigger;
  $(document).on('click', '.product-detail-link', function (event) {
    if (event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
    event.preventDefault();
    drawerTrigger = this;
    var href = $(this).attr('href'), current = ++drawerSequence;
    if (drawerRequest) drawerRequest.abort();
    var body = $('#product-drawer-body').empty().append($('<p>', {class:'loading-text',text:'Loading product details…'}));
    bootstrap.Offcanvas.getOrCreateInstance(document.getElementById('product-drawer')).show();
    drawerRequest = $.ajax({url:href, data:{partial:'true'}, dataType:'html'});
    drawerRequest.done(function (html) { if (current === drawerSequence) body.html(html); }); // HTML is an encoded Razor partial, never raw API values.
    drawerRequest.fail(function (_, status) {
      if (current !== drawerSequence || status === 'abort') return;
      body.empty().append($('<p>', {role:'alert',text:'Unable to load product details. Please try again.'}), $('<a>',{href:href,text:'Open full product page'}));
    });
  });
  $('#product-drawer').on('hidden.bs.offcanvas', function () { drawerSequence++; if (drawerRequest) drawerRequest.abort(); if (drawerTrigger && drawerTrigger.isConnected) drawerTrigger.focus(); });
  var form = $('#catalog-filters');
  if (!form.length) return;
  var pending, sequence = 0, timer;
  var inventory = form.data('inventory') === true;
  function load(page) {
    var current = ++sequence;
    if (pending) pending.abort();
    $('#filter-status').text('Loading products…');
    $('#product-rows').attr('aria-busy', 'true');
    var query = form.serialize() + '&page=' + (page || 1) + '&inventory=' + inventory;
    pending = $.ajax({url:form.data('endpoint'), data:query, dataType:'json'});
    pending.done(function (result) {
      if (current !== sequence) return;
      var products = result.products.map(function (row) {
        return Object.assign({}, row, {detailUrl:'/Admin/Product/' + encodeURIComponent(row.id)});
      });
      $('#product-rows').html(Handlebars.templates.products({products:products,inventory:inventory}));
      $('#result-pagination').html(Handlebars.templates.pagination(result));
      $('#filter-status').text(result.pager.total + ' products found.');
      history.replaceState(null, '', location.pathname + '?' + query);
    }).fail(function (_, status) {
      if (current !== sequence || status === 'abort') return;
      $('#filter-status').text('Unable to refresh products. Previous results remain visible. Apply filters to retry.');
    }).always(function () { if (current === sequence) $('#product-rows').removeAttr('aria-busy'); });
  }
  form.on('submit', function (event) { event.preventDefault(); clearTimeout(timer); load(1); });
  form.find('select').on('change', function () { clearTimeout(timer); load(1); });
  form.find('input').on('input', function () { clearTimeout(timer); timer = setTimeout(function () { load(1); }, 300); });
  $('#result-pagination').on('click', 'a', function (event) {
    if (event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
    event.preventDefault(); clearTimeout(timer);
    load(new URL($(this).attr('href'), location.origin).searchParams.get('page'));
  });
})(jQuery);
