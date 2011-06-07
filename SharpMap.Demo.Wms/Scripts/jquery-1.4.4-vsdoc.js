var jQuery = $ = function(selector, context){
		/// <summary>
		/// 	Accepts a string containing a CSS selector which is then used to match a set of elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. jQuery( element )
		/// 	&#10;&#09;2. jQuery( elementArray )
		/// 	&#10;&#09;3. jQuery( jQuery object )
		/// 	&#10;&#09;4. jQuery(  )
		/// 	&#10;&#09;5. jQuery( html, [ownerDocument] )
		/// 	&#10;&#09;6. jQuery( html, props )
		/// 	&#10;&#09;7. jQuery( callback )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery
		/// </summary>
		///	<param name="selector" type="String">
		/// 	A string containing a selector expression
		/// </param>
		///	<param name="context" type="jQuery" optional="true">
		/// 	A DOM Element, Document, or jQuery to use as context
		/// </param>
		/// <returns type="jQuery" />
};
$.prototype = {

	fadeToggle: function(duration, easing, callback){
		/// <summary>
		/// 	Display or hide the matched elements by animating their opacity.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/fadeToggle
		/// </summary>
		///	<param name="duration" type="Number" optional="true">
		/// 	A string or number determining how long the animation will run.
		/// </param>
		///	<param name="easing" type="String" optional="true">
		/// 	A string indicating which easing function to use for the transition.
		/// </param>
		///	<param name="callback" type="Function" optional="true">
		/// 	A function to call once the animation is complete.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	toggle: function(handler, handler1, handler2){
		/// <summary>
		/// 	Bind two or more handlers to the matched elements, to be executed on alternate clicks.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/toggle
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute every even time the element is clicked.
		/// </param>
		///	<param name="handler1" type="Function">
		/// 	A function to execute every odd time the element is clicked.
		/// </param>
		///	<param name="handler2" type="Function" optional="true">
		/// 	Additional handlers to cycle through after clicks.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	undelegate: function(){
		/// <summary>
		/// 	Remove a handler from the event for all elements which match the current selector, now or in the future, based upon a specific set of root elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .undelegate( selector, eventType )
		/// 	&#10;&#09;2. .undelegate( selector, eventType, handler )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/undelegate
		/// </summary>
		/// <returns type="jQuery" />
	}, 
	delegate: function(selector, eventType, handler){
		/// <summary>
		/// 	Attach a handler to one or more events for all elements that match the selector, now or in the future, based on a specific set of root elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .delegate( selector, eventType, eventData, handler )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/delegate
		/// </summary>
		///	<param name="selector" type="String">
		/// 	A selector to filter the elements that trigger the event.
		/// </param>
		///	<param name="eventType" type="String">
		/// 	A string containing one or more space-separated JavaScript event types, such as "click" or "keydown," or custom event names.
		/// </param>
		///	<param name="handler" type="Function">
		/// 	A function to execute at the time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	focusout: function(handler){
		/// <summary>
		/// 	Bind an event handler to the "focusout" JavaScript event.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .focusout( [eventData], handler )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/focusout
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute each time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	focusin: function(handler){
		/// <summary>
		/// 	Bind an event handler to the "focusin" JavaScript event.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .focusin( [eventData], handler )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/focusin
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute each time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	has: function(selector){
		/// <summary>
		/// 	Reduce the set of matched elements to those that have a descendant that matches the selector or DOM element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .has( contained )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/has
		/// </summary>
		///	<param name="selector" type="String">
		/// 	A string containing a selector expression to match elements against.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	delay: function(duration, queueName){
		/// <summary>
		/// 	Set a timer to delay execution of subsequent items in the queue.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/delay
		/// </summary>
		///	<param name="duration" type="Number" integer="true">
		/// 	An integer indicating the number of milliseconds to delay execution of the next item in the queue.
		/// </param>
		///	<param name="queueName" type="String" optional="true">
		/// 	A string containing the name of the queue. Defaults to fx, the standard effects queue.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	parentsUntil: function(selector){
		/// <summary>
		/// 	Get the ancestors of each element in the current set of matched elements, up to but not including the element matched by the selector.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/parentsUntil
		/// </summary>
		///	<param name="selector" type="String" optional="true">
		/// 	A string containing a selector expression to indicate where to stop matching ancestor elements.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	prevUntil: function(selector){
		/// <summary>
		/// 	Get all preceding siblings of each element up to but not including the element matched by the selector.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/prevUntil
		/// </summary>
		///	<param name="selector" type="String" optional="true">
		/// 	A string containing a selector expression to indicate where to stop matching preceding sibling elements.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	nextUntil: function(selector){
		/// <summary>
		/// 	Get all following siblings of each element up to but not including the element matched by the selector.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/nextUntil
		/// </summary>
		///	<param name="selector" type="String" optional="true">
		/// 	A string containing a selector expression to indicate where to stop matching following sibling elements.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	each: function(method){
		/// <summary>
		/// 	Iterate over a jQuery object, executing a function for each matched element. 
		/// 	&#10;&#10;API Reference: http://api.jquery.com/each
		/// </summary>
		///	<param name="method" type="Function">
		/// 	A function to execute for each matched element.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	pushStack: function(elements){
		/// <summary>
		/// 	Add a collection of DOM elements onto the jQuery stack.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .pushStack( elements, name, arguments )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/pushStack
		/// </summary>
		///	<param name="elements" type="Array">
		/// 	An array of elements to push onto the stack and make into a new jQuery object.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	clearQueue: function(queueName){
		/// <summary>
		/// 	Remove from the queue all items that have not yet been run.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/clearQueue
		/// </summary>
		///	<param name="queueName" type="String" optional="true">
		/// 	A string containing the name of the queue. Defaults to fx, the standard effects queue.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	toArray: function(){
		/// <summary>
		/// 	Retrieve all the DOM elements contained in the jQuery set, as an array.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/toArray
		/// </summary>
		/// <returns type="Array" />
	}, 
	keydown: function(handler){
		/// <summary>
		/// 	Bind an event handler to the "keydown" JavaScript event, or trigger that event on an element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .keydown( [eventData], handler )
		/// 	&#10;&#09;2. .keydown(  )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/keydown
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute each time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	index: function(){
		/// <summary>
		/// 	Search for a given element from among the matched elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .index( selector )
		/// 	&#10;&#09;2. .index( element )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/index
		/// </summary>
		/// <returns type="Number" />
	}, 
	removeData: function(name){
		/// <summary>
		/// 	Remove a previously-stored piece of data.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/removeData
		/// </summary>
		///	<param name="name" type="String" optional="true">
		/// 	A string naming the piece of data to delete.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	data: function(key, value){
		/// <summary>
		/// 	Store arbitrary data associated with the matched elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .data( obj )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/data
		/// </summary>
		///	<param name="key" type="String">
		/// 	A string naming the piece of data to set.
		/// </param>
		///	<param name="value" type="Object">
		/// 	The new data value; it can be any Javascript type including Array or Object.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	data: function(key){
		/// <summary>
		/// 	Returns value at named data store for the first element in the jQuery collection, as set by data(name, value).
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .data(  )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/data
		/// </summary>
		///	<param name="key" type="String">
		/// 	Name of the data stored.
		/// </param>
		/// <returns type="Object" />
	}, 
	get: function(index){
		/// <summary>
		/// 	Retrieve the DOM elements matched by the jQuery object.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/get
		/// </summary>
		///	<param name="index" type="Number" optional="true">
		/// 	A zero-based integer indicating which element to retrieve.
		/// </param>
		/// <returns type="Array" />
	}, 
	size: function(){
		/// <summary>
		/// 	Return the number of DOM elements matched by the jQuery object.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/size
		/// </summary>
		/// <returns type="Number" />
	}, 
	scroll: function(handler){
		/// <summary>
		/// 	Bind an event handler to the "scroll" JavaScript event, or trigger that event on an element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .scroll( [eventData], handler )
		/// 	&#10;&#09;2. .scroll(  )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/scroll
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute each time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	resize: function(handler){
		/// <summary>
		/// 	Bind an event handler to the "resize" JavaScript event, or trigger that event on an element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .resize( [eventData], handler )
		/// 	&#10;&#09;2. .resize(  )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/resize
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute each time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	dequeue: function(queueName){
		/// <summary>
		/// 	Execute the next function on the queue for the matched elements.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/dequeue
		/// </summary>
		///	<param name="queueName" type="String" optional="true">
		/// 	A string containing the name of the queue. Defaults to fx, the standard effects queue.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	queue: function(queueName){
		/// <summary>
		/// 	Show the queue of functions to be executed on the matched elements.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/queue
		/// </summary>
		///	<param name="queueName" type="String" optional="true">
		/// 	A string containing the name of the queue. Defaults to fx, the standard effects queue.
		/// </param>
		/// <returns type="Array" />
	}, 
	queue: function(queueName, newQueue){
		/// <summary>
		/// 	Manipulate the queue of functions to be executed on the matched elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .queue( [queueName], method )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/queue
		/// </summary>
		///	<param name="queueName" type="String" optional="true">
		/// 	A string containing the name of the queue. Defaults to fx, the standard effects queue.
		/// </param>
		///	<param name="newQueue" type="Array">
		/// 	An array of functions to replace the current queue contents.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	keyup: function(handler){
		/// <summary>
		/// 	Bind an event handler to the "keyup" JavaScript event, or trigger that event on an element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .keyup( [eventData], handler )
		/// 	&#10;&#09;2. .keyup(  )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/keyup
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute each time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	keypress: function(handler){
		/// <summary>
		/// 	Bind an event handler to the "keypress" JavaScript event, or trigger that event on an element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .keypress( [eventData], handler )
		/// 	&#10;&#09;2. .keypress(  )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/keypress
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute each time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	submit: function(handler){
		/// <summary>
		/// 	Bind an event handler to the "submit" JavaScript event, or trigger that event on an element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .submit( [eventData], handler )
		/// 	&#10;&#09;2. .submit(  )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/submit
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute each time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	select: function(handler){
		/// <summary>
		/// 	Bind an event handler to the "select" JavaScript event, or trigger that event on an element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .select( [eventData], handler )
		/// 	&#10;&#09;2. .select(  )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/select
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute each time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	change: function(handler){
		/// <summary>
		/// 	Bind an event handler to the "change" JavaScript event, or trigger that event on an element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .change( [eventData], handler )
		/// 	&#10;&#09;2. .change(  )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/change
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute each time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	blur: function(handler){
		/// <summary>
		/// 	Bind an event handler to the "blur" JavaScript event, or trigger that event on an element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .blur( [eventData], handler )
		/// 	&#10;&#09;2. .blur(  )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/blur
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute each time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	focus: function(handler){
		/// <summary>
		/// 	Bind an event handler to the "focus" JavaScript event, or trigger that event on an element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .focus( [eventData], handler )
		/// 	&#10;&#09;2. .focus(  )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/focus
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute each time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	mousemove: function(handler){
		/// <summary>
		/// 	Bind an event handler to the "mousemove" JavaScript event, or trigger that event on an element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .mousemove( [eventData], handler )
		/// 	&#10;&#09;2. .mousemove(  )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/mousemove
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute each time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	hover: function(handler, handler1){
		/// <summary>
		/// 	Bind two handlers to the matched elements, to be executed when the mouse pointer enters and leaves the elements.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/hover
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute when the mouse pointer enters the element.
		/// </param>
		///	<param name="handler1" type="Function">
		/// 	A function to execute when the mouse pointer leaves the element.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	hover: function(handler){
		/// <summary>
		/// 	Bind a single handler to the matched elements, to be executed when the mouse pointer enters or leaves the elements.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/hover
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute when the mouse pointer enters or leaves the element.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	mouseleave: function(handler){
		/// <summary>
		/// 	Bind an event handler to be fired when the mouse leaves an element, or trigger that handler on an element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .mouseleave( [eventData], handler )
		/// 	&#10;&#09;2. .mouseleave(  )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/mouseleave
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute each time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	mouseenter: function(handler){
		/// <summary>
		/// 	Bind an event handler to be fired when the mouse enters an element, or trigger that handler on an element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .mouseenter( [eventData], handler )
		/// 	&#10;&#09;2. .mouseenter(  )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/mouseenter
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute each time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	mouseout: function(handler){
		/// <summary>
		/// 	Bind an event handler to the "mouseout" JavaScript event, or trigger that event on an element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .mouseout( [eventData], handler )
		/// 	&#10;&#09;2. .mouseout(  )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/mouseout
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute each time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	mouseover: function(handler){
		/// <summary>
		/// 	Bind an event handler to the "mouseover" JavaScript event, or trigger that event on an element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .mouseover( [eventData], handler )
		/// 	&#10;&#09;2. .mouseover(  )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/mouseover
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute each time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	dblclick: function(handler){
		/// <summary>
		/// 	Bind an event handler to the "dblclick" JavaScript event, or trigger that event on an element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .dblclick( [eventData], handler )
		/// 	&#10;&#09;2. .dblclick(  )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/dblclick
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute each time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	click: function(handler){
		/// <summary>
		/// 	Bind an event handler to the "click" JavaScript event, or trigger that event on an element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .click( [eventData], handler )
		/// 	&#10;&#09;2. .click(  )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/click
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute each time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	mouseup: function(handler){
		/// <summary>
		/// 	Bind an event handler to the "mouseup" JavaScript event, or trigger that event on an element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .mouseup( [eventData], handler )
		/// 	&#10;&#09;2. .mouseup(  )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/mouseup
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute each time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	mousedown: function(handler){
		/// <summary>
		/// 	Bind an event handler to the "mousedown" JavaScript event, or trigger that event on an element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .mousedown( [eventData], handler )
		/// 	&#10;&#09;2. .mousedown(  )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/mousedown
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute each time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	error: function(handler){
		/// <summary>
		/// 	Bind an event handler to the "error" JavaScript event.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .error( [eventData], handler )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/error
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute when the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	unload: function(handler){
		/// <summary>
		/// 	Bind an event handler to the "unload" JavaScript event.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .unload( [eventData], handler )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/unload
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute when the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	load: function(handler){
		/// <summary>
		/// 	Bind an event handler to the "load" JavaScript event.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .load( [eventData], handler )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/load
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute when the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	ready: function(handler){
		/// <summary>
		/// 	Specify a function to execute when the DOM is fully loaded.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/ready
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	A function to execute after the DOM is ready.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	die: function(){
		/// <summary>
		/// 	Remove all event handlers previously attached using .live() from the elements.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/die
		/// </summary>
		/// <returns type="jQuery" />
	}, 
	die: function(eventType, handler){
		/// <summary>
		/// 	Remove an event handler previously attached using .live() from the elements.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/die
		/// </summary>
		///	<param name="eventType" type="String">
		/// 	A string containing a JavaScript event type, such as click or keydown.
		/// </param>
		///	<param name="handler" type="String" optional="true">
		/// 	The function that is to be no longer executed.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	live: function(eventType, handler){
		/// <summary>
		/// 	Attach a handler to the event for all elements which match the current selector, now and in the future.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .live( eventType, eventData, handler )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/live
		/// </summary>
		///	<param name="eventType" type="String">
		/// 	A string containing a JavaScript event type, such as "click" or "keydown." As of jQuery 1.4 the string can contain multiple, space-separated event types or custom event names, as well.
		/// </param>
		///	<param name="handler" type="Function">
		/// 	A function to execute at the time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	triggerHandler: function(eventType, extraParameters){
		/// <summary>
		/// 	Execute all handlers attached to an element for an event.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/triggerHandler
		/// </summary>
		///	<param name="eventType" type="String">
		/// 	A string containing a JavaScript event type, such as click or submit.
		/// </param>
		///	<param name="extraParameters" type="Array">
		/// 	An array of additional parameters to pass along to the event handler.
		/// </param>
		/// <returns type="Object" />
	}, 
	trigger: function(eventType, extraParameters){
		/// <summary>
		/// 	Execute all handlers and behaviors attached to the matched elements for the given event type.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .trigger( event )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/trigger
		/// </summary>
		///	<param name="eventType" type="String">
		/// 	A string containing a JavaScript event type, such as click or submit.
		/// </param>
		///	<param name="extraParameters" type="Array">
		/// 	An array of additional parameters to pass along to the event handler.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	ajaxComplete: function(handler){
		/// <summary>
		/// 	Register a handler to be called when Ajax requests complete. This is an Ajax Event.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/ajaxComplete
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	The function to be invoked.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	one: function(eventType, eventData, handler){
		/// <summary>
		/// 	Attach a handler to an event for the elements. The handler is executed at most once per element.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/one
		/// </summary>
		///	<param name="eventType" type="String">
		/// 	A string containing one or more JavaScript event types, such as "click" or "submit," or custom event names.
		/// </param>
		///	<param name="eventData" type="Object" optional="true">
		/// 	A map of data that will be passed to the event handler.
		/// </param>
		///	<param name="handler" type="Function">
		/// 	A function to execute at the time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	serializeArray: function(){
		/// <summary>
		/// 	Encode a set of form elements as an array of names and values.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/serializeArray
		/// </summary>
		/// <returns type="Array" />
	}, 
	serialize: function(){
		/// <summary>
		/// 	Encode a set of form elements as a string for submission.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/serialize
		/// </summary>
		/// <returns type="String" />
	}, 
	ajaxSuccess: function(handler){
		/// <summary>
		/// 	Attach a function to be executed whenever an Ajax request completes successfully. This is an Ajax Event.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/ajaxSuccess
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	The function to be invoked.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	ajaxStop: function(handler){
		/// <summary>
		/// 	Register a handler to be called when all Ajax requests have completed. This is an Ajax Event.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/ajaxStop
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	The function to be invoked.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	ajaxStart: function(handler){
		/// <summary>
		/// 	Register a handler to be called when the first Ajax request begins. This is an Ajax Event.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/ajaxStart
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	The function to be invoked.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	ajaxSend: function(handler){
		/// <summary>
		/// 	Attach a function to be executed before an Ajax request is sent. This is an Ajax Event.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/ajaxSend
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	The function to be invoked.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	ajaxError: function(handler){
		/// <summary>
		/// 	Register a handler to be called when Ajax requests complete with an error. This is an Ajax Event.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/ajaxError
		/// </summary>
		///	<param name="handler" type="Function">
		/// 	The function to be invoked.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	unbind: function(eventType, handler){
		/// <summary>
		/// 	Remove a previously-attached event handler from the elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .unbind( eventType, false )
		/// 	&#10;&#09;2. .unbind( event )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/unbind
		/// </summary>
		///	<param name="eventType" type="String" optional="true">
		/// 	A string containing a JavaScript event type, such as click or submit.
		/// </param>
		///	<param name="handler" type="Function" optional="true">
		/// 	The function that is to be no longer executed.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	bind: function(eventType, eventData, handler){
		/// <summary>
		/// 	Attach a handler to an event for the elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .bind( eventType, [eventData], false )
		/// 	&#10;&#09;2. .bind( events )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/bind
		/// </summary>
		///	<param name="eventType" type="String">
		/// 	A string containing one or more JavaScript event types, such as "click" or "submit," or custom event names.
		/// </param>
		///	<param name="eventData" type="Object" optional="true">
		/// 	A map of data that will be passed to the event handler.
		/// </param>
		///	<param name="handler" type="Function">
		/// 	A function to execute each time the event is triggered.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	first: function(){
		/// <summary>
		/// 	Reduce the set of matched elements to the first in the set.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/first
		/// </summary>
		/// <returns type="jQuery" />
	}, 
	last: function(){
		/// <summary>
		/// 	Reduce the set of matched elements to the final one in the set.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/last
		/// </summary>
		/// <returns type="jQuery" />
	}, 
	slice: function(start, end){
		/// <summary>
		/// 	Reduce the set of matched elements to a subset specified by a range of indices.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/slice
		/// </summary>
		///	<param name="start" type="Number" integer="true">
		/// 	An integer indicating the 0-based position at which the elements begin to be selected. If negative, it indicates an offset from the end of the set.
		/// </param>
		///	<param name="end" type="Number" optional="true" integer="true">
		/// 	An integer indicating the 0-based position at which the elements stop being selected. If negative, it indicates an offset from the end of the set. If omitted, the range continues until the end of the set.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	stop: function(clearQueue, jumpToEnd){
		/// <summary>
		/// 	Stop the currently-running animation on the matched elements.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/stop
		/// </summary>
		///	<param name="clearQueue" type="Boolean" optional="true">
		/// 	A Boolean indicating whether to remove queued animation as well. Defaults to false.
		/// </param>
		///	<param name="jumpToEnd" type="Boolean" optional="true">
		/// 	A Boolean indicating whether to complete the current animation immediately. Defaults to false.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	end: function(){
		/// <summary>
		/// 	End the most recent filtering operation in the current chain and return the set of matched elements to its previous state.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/end
		/// </summary>
		/// <returns type="jQuery" />
	}, 
	andSelf: function(){
		/// <summary>
		/// 	Add the previous set of elements on the stack to the current set.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/andSelf
		/// </summary>
		/// <returns type="jQuery" />
	}, 
	siblings: function(selector){
		/// <summary>
		/// 	Get the siblings of each element in the set of matched elements, optionally filtered by a selector.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/siblings
		/// </summary>
		///	<param name="selector" type="String" optional="true">
		/// 	A string containing a selector expression to match elements against.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	animate: function(properties, duration, easing, callback){
		/// <summary>
		/// 	Perform a custom animation of a set of CSS properties.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .animate( properties, options )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/animate
		/// </summary>
		///	<param name="properties" type="Object">
		/// 	A map of CSS properties that the animation will move toward.
		/// </param>
		///	<param name="duration" type="Number" optional="true">
		/// 	A string or number determining how long the animation will run.
		/// </param>
		///	<param name="easing" type="String" optional="true">
		/// 	A string indicating which easing function to use for the transition.
		/// </param>
		///	<param name="callback" type="Function" optional="true">
		/// 	A function to call once the animation is complete.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	prevAll: function(selector){
		/// <summary>
		/// 	Get all preceding siblings of each element in the set of matched elements, optionally filtered by a selector.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/prevAll
		/// </summary>
		///	<param name="selector" type="String" optional="true">
		/// 	A string containing a selector expression to match elements against.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	prev: function(selector){
		/// <summary>
		/// 	Get the immediately preceding sibling of each element in the set of matched elements, optionally filtered by a selector.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/prev
		/// </summary>
		///	<param name="selector" type="String" optional="true">
		/// 	A string containing a selector expression to match elements against.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	fadeTo: function(duration, opacity, callback){
		/// <summary>
		/// 	Adjust the opacity of the matched elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .fadeTo( [duration], opacity, [easing], [callback] )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/fadeTo
		/// </summary>
		///	<param name="duration" type="Number">
		/// 	A string or number determining how long the animation will run.
		/// </param>
		///	<param name="opacity" type="Number">
		/// 	A number between 0 and 1 denoting the target opacity.
		/// </param>
		///	<param name="callback" type="Function" optional="true">
		/// 	A function to call once the animation is complete.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	fadeOut: function(duration, callback){
		/// <summary>
		/// 	Hide the matched elements by fading them to transparent.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .fadeOut( [duration], [easing], [callback] )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/fadeOut
		/// </summary>
		///	<param name="duration" type="Number" optional="true">
		/// 	A string or number determining how long the animation will run.
		/// </param>
		///	<param name="callback" type="Function" optional="true">
		/// 	A function to call once the animation is complete.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	parents: function(selector){
		/// <summary>
		/// 	Get the ancestors of each element in the current set of matched elements, optionally filtered by a selector.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/parents
		/// </summary>
		///	<param name="selector" type="String" optional="true">
		/// 	A string containing a selector expression to match elements against.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	fadeIn: function(duration, callback){
		/// <summary>
		/// 	Display the matched elements by fading them to opaque.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .fadeIn( [duration], [easing], [callback] )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/fadeIn
		/// </summary>
		///	<param name="duration" type="Number" optional="true">
		/// 	A string or number determining how long the animation will run.
		/// </param>
		///	<param name="callback" type="Function" optional="true">
		/// 	A function to call once the animation is complete.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	parent: function(selector){
		/// <summary>
		/// 	Get the parent of each element in the current set of matched elements, optionally filtered by a selector.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/parent
		/// </summary>
		///	<param name="selector" type="String" optional="true">
		/// 	A string containing a selector expression to match elements against.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	offsetParent: function(){
		/// <summary>
		/// 	Get the closest ancestor element that is positioned.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/offsetParent
		/// </summary>
		/// <returns type="jQuery" />
	}, 
	slideToggle: function(duration, callback){
		/// <summary>
		/// 	Display or hide the matched elements with a sliding motion.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .slideToggle( [duration], [easing], [callback] )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/slideToggle
		/// </summary>
		///	<param name="duration" type="Number" optional="true">
		/// 	A string or number determining how long the animation will run.
		/// </param>
		///	<param name="callback" type="Function" optional="true">
		/// 	A function to call once the animation is complete.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	slideUp: function(duration, callback){
		/// <summary>
		/// 	Hide the matched elements with a sliding motion.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .slideUp( [duration], [easing], [callback] )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/slideUp
		/// </summary>
		///	<param name="duration" type="Number" optional="true">
		/// 	A string or number determining how long the animation will run.
		/// </param>
		///	<param name="callback" type="Function" optional="true">
		/// 	A function to call once the animation is complete.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	nextAll: function(selector){
		/// <summary>
		/// 	Get all following siblings of each element in the set of matched elements, optionally filtered by a selector.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/nextAll
		/// </summary>
		///	<param name="selector" type="String" optional="true">
		/// 	A string containing a selector expression to match elements against.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	next: function(selector){
		/// <summary>
		/// 	Get the immediately following sibling of each element in the set of matched elements. If a selector is provided, it retrieves the next sibling only if it matches that selector.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/next
		/// </summary>
		///	<param name="selector" type="String" optional="true">
		/// 	A string containing a selector expression to match elements against.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	slideDown: function(duration, callback){
		/// <summary>
		/// 	Display the matched elements with a sliding motion.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .slideDown( [duration], [easing], [callback] )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/slideDown
		/// </summary>
		///	<param name="duration" type="Number" optional="true">
		/// 	A string or number determining how long the animation will run.
		/// </param>
		///	<param name="callback" type="Function" optional="true">
		/// 	A function to call once the animation is complete.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	find: function(selector){
		/// <summary>
		/// 	Get the descendants of each element in the current set of matched elements, filtered by a selector.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/find
		/// </summary>
		///	<param name="selector" type="String">
		/// 	A string containing a selector expression to match elements against.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	contents: function(){
		/// <summary>
		/// 	Get the children of each element in the set of matched elements, including text and comment nodes.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/contents
		/// </summary>
		/// <returns type="jQuery" />
	}, 
	closest: function(selector){
		/// <summary>
		/// 	Get the first ancestor element that matches the selector, beginning at the current element and progressing up through the DOM tree.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .closest( selector, [context] )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/closest
		/// </summary>
		///	<param name="selector" type="String">
		/// 	A string containing a selector expression to match elements against.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	closest: function(selectors, context){
		/// <summary>
		/// 	Gets an array of all the elements and selectors matched against the current element up through the DOM tree.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/closest
		/// </summary>
		///	<param name="selectors" type="Array">
		/// 	An array or string containing a selector expression to match elements against (can also be a jQuery object).
		/// </param>
		///	<param name="context" type="Element" optional="true">
		/// 	A DOM element within which a matching element may be found. If no context is passed in then the context of the jQuery set will be used instead.
		/// </param>
		/// <returns type="Array" />
	}, 
	load: function(url, data, method){
		/// <summary>
		/// 	Load data from the server and place the returned HTML into the matched element.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/load
		/// </summary>
		///	<param name="url" type="String">
		/// 	A string containing the URL to which the request is sent.
		/// </param>
		///	<param name="data" type="String" optional="true">
		/// 	A map or string that is sent to the server with the request.
		/// </param>
		///	<param name="method" type="Function" optional="true">
		/// 	A callback function that is executed when the request completes.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	children: function(selector){
		/// <summary>
		/// 	Get the children of each element in the set of matched elements, optionally filtered by a selector.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/children
		/// </summary>
		///	<param name="selector" type="String" optional="true">
		/// 	A string containing a selector expression to match elements against.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	add: function(selector){
		/// <summary>
		/// 	Add elements to the set of matched elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .add( elements )
		/// 	&#10;&#09;2. .add( html )
		/// 	&#10;&#09;3. .add( selector, context )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/add
		/// </summary>
		///	<param name="selector" type="String">
		/// 	A string containing a selector expression to match additional elements against.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	not: function(selector){
		/// <summary>
		/// 	Remove elements from the set of matched elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .not( elements )
		/// 	&#10;&#09;2. .not( method )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/not
		/// </summary>
		///	<param name="selector" type="String">
		/// 	A string containing a selector expression to match elements against.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	outerWidth: function(includeMargin){
		/// <summary>
		/// 	Get the current computed width for the first element in the set of matched elements, including padding and border.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/outerWidth
		/// </summary>
		///	<param name="includeMargin" type="Boolean" optional="true">
		/// 	A Boolean indicating whether to include the element's margin in the calculation.
		/// </param>
		/// <returns type="Number" />
	}, 
	outerHeight: function(includeMargin){
		/// <summary>
		/// 	Get the current computed height for the first element in the set of matched elements, including padding, border, and optionally margin.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/outerHeight
		/// </summary>
		///	<param name="includeMargin" type="Boolean" optional="true">
		/// 	A Boolean indicating whether to include the element's margin in the calculation.
		/// </param>
		/// <returns type="Number" />
	}, 
	toggle: function(duration, callback){
		/// <summary>
		/// 	Display or hide the matched elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .toggle( [duration], [easing], [callback] )
		/// 	&#10;&#09;2. .toggle( showOrHide )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/toggle
		/// </summary>
		///	<param name="duration" type="Number" optional="true">
		/// 	A string or number determining how long the animation will run.
		/// </param>
		///	<param name="callback" type="Function" optional="true">
		/// 	A function to call once the animation is complete.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	innerWidth: function(){
		/// <summary>
		/// 	Get the current computed width for the first element in the set of matched elements, including padding but not border.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/innerWidth
		/// </summary>
		/// <returns type="Number" />
	}, 
	innerHeight: function(){
		/// <summary>
		/// 	Get the current computed height for the first element in the set of matched elements, including padding but not border.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/innerHeight
		/// </summary>
		/// <returns type="Number" />
	}, 
	hide: function(){
		/// <summary>
		/// 	Hide the matched elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .hide( duration, [callback] )
		/// 	&#10;&#09;2. .hide( [duration], [easing], [callback] )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/hide
		/// </summary>
		/// <returns type="jQuery" />
	}, 
	width: function(){
		/// <summary>
		/// 	Get the current computed width for the first element in the set of matched elements.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/width
		/// </summary>
		/// <returns type="Number" />
	}, 
	width: function(value){
		/// <summary>
		/// 	Set the CSS width of each element in the set of matched elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .width( method )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/width
		/// </summary>
		///	<param name="value" type="Number">
		/// 	An integer representing the number of pixels, or an integer along with an optional unit of measure appended (as a string).
		/// </param>
		/// <returns type="jQuery" />
	}, 
	height: function(){
		/// <summary>
		/// 	Get the current computed height for the first element in the set of matched elements.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/height
		/// </summary>
		/// <returns type="Number" />
	}, 
	height: function(value){
		/// <summary>
		/// 	Set the CSS height of every matched element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .height( method )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/height
		/// </summary>
		///	<param name="value" type="Number">
		/// 	An integer representing the number of pixels, or an integer with an optional unit of measure appended (as a string).
		/// </param>
		/// <returns type="jQuery" />
	}, 
	show: function(){
		/// <summary>
		/// 	Display the matched elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .show( duration, [callback] )
		/// 	&#10;&#09;2. .show( [duration], [easing], [callback] )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/show
		/// </summary>
		/// <returns type="jQuery" />
	}, 
	scrollLeft: function(){
		/// <summary>
		/// 	Get the current horizontal position of the scroll bar for the first element in the set of matched elements.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/scrollLeft
		/// </summary>
		/// <returns type="Number" />
	}, 
	scrollLeft: function(value){
		/// <summary>
		/// 	Set the current horizontal position of the scroll bar for each of the set of matched elements.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/scrollLeft
		/// </summary>
		///	<param name="value" type="Number">
		/// 	An integer indicating the new position to set the scroll bar to.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	scrollTop: function(){
		/// <summary>
		/// 	Get the current vertical position of the scroll bar for the first element in the set of matched elements.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/scrollTop
		/// </summary>
		/// <returns type="Number" />
	}, 
	scrollTop: function(value){
		/// <summary>
		/// 	Set the current vertical position of the scroll bar for each of the set of matched elements.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/scrollTop
		/// </summary>
		///	<param name="value" type="Number">
		/// 	An integer indicating the new position to set the scroll bar to.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	position: function(){
		/// <summary>
		/// 	Get the current coordinates of the first element in the set of matched elements, relative to the offset parent.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/position
		/// </summary>
		/// <returns type="Object" />
	}, 
	offset: function(){
		/// <summary>
		/// 	Get the current coordinates of the first element in the set of matched elements, relative to the document.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/offset
		/// </summary>
		/// <returns type="Object" />
	}, 
	offset: function(coordinates){
		/// <summary>
		/// 	Set the current coordinates of every element in the set of matched elements, relative to the document.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .offset( method )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/offset
		/// </summary>
		///	<param name="coordinates" type="Object">
		/// 	An object containing the properties top and left, which are integers indicating the new top and left coordinates for the elements.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	css: function(propertyName){
		/// <summary>
		/// 	Get the value of a style property for the first element in the set of matched elements.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/css
		/// </summary>
		///	<param name="propertyName" type="String">
		/// 	A CSS property.
		/// </param>
		/// <returns type="String" />
	}, 
	css: function(propertyName, value){
		/// <summary>
		/// 	Set one or more CSS properties for the  set of matched elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .css( propertyName, method )
		/// 	&#10;&#09;2. .css( map )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/css
		/// </summary>
		///	<param name="propertyName" type="String">
		/// 	A CSS property name.
		/// </param>
		///	<param name="value" type="Number">
		/// 	A value to set for the property.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	unwrap: function(){
		/// <summary>
		/// 	Remove the parents of the set of matched elements from the DOM, leaving the matched elements in their place.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/unwrap
		/// </summary>
		/// <returns type="jQuery" />
	}, 
	detach: function(selector){
		/// <summary>
		/// 	Remove the set of matched elements from the DOM.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/detach
		/// </summary>
		///	<param name="selector" type="String" optional="true">
		/// 	A selector expression that filters the set of matched elements to be removed.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	clone: function(withDataAndEvents){
		/// <summary>
		/// 	Create a deep copy of the set of matched elements.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/clone
		/// </summary>
		///	<param name="withDataAndEvents" type="Boolean" optional="true">
		/// 	A Boolean indicating whether event handlers should be copied along with the elements. As of jQuery 1.4 element data will be copied as well.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	remove: function(selector){
		/// <summary>
		/// 	Remove the set of matched elements from the DOM.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/remove
		/// </summary>
		///	<param name="selector" type="String" optional="true">
		/// 	A selector expression that filters the set of matched elements to be removed.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	empty: function(){
		/// <summary>
		/// 	Remove all child nodes of the set of matched elements from the DOM.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/empty
		/// </summary>
		/// <returns type="jQuery" />
	}, 
	replaceAll: function(){
		/// <summary>
		/// 	Replace each target element with the set of matched elements.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/replaceAll
		/// </summary>
		/// <returns type="jQuery" />
	}, 
	replaceWith: function(newContent){
		/// <summary>
		/// 	Replace each element in the set of matched elements with the provided new content.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .replaceWith( function )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/replaceWith
		/// </summary>
		///	<param name="newContent" type="jQuery">
		/// 	The content to insert. May be an HTML string, DOM element, or jQuery object.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	wrapInner: function(wrappingElement){
		/// <summary>
		/// 	Wrap an HTML structure around the content of each element in the set of matched elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .wrapInner( wrappingFunction )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/wrapInner
		/// </summary>
		///	<param name="wrappingElement" type="String">
		/// 	An HTML snippet, selector expression, jQuery object, or DOM element specifying the structure to wrap around the content of the matched elements.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	wrapAll: function(wrappingElement){
		/// <summary>
		/// 	Wrap an HTML structure around all elements in the set of matched elements.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/wrapAll
		/// </summary>
		///	<param name="wrappingElement" type="jQuery">
		/// 	An HTML snippet, selector expression, jQuery object, or DOM element specifying the structure to wrap around the matched elements.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	wrap: function(wrappingElement){
		/// <summary>
		/// 	Wrap an HTML structure around each element in the set of matched elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .wrap( wrappingFunction )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/wrap
		/// </summary>
		///	<param name="wrappingElement" type="jQuery">
		/// 	An HTML snippet, selector expression, jQuery object, or DOM element specifying the structure to wrap around the matched elements.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	insertBefore: function(target){
		/// <summary>
		/// 	Insert every element in the set of matched elements before the target.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/insertBefore
		/// </summary>
		///	<param name="target" type="jQuery">
		/// 	A selector, element, HTML string, or jQuery object; the matched set of elements will be inserted before the element(s) specified by this parameter.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	before: function(content){
		/// <summary>
		/// 	Insert content, specified by the parameter, before each element in the set of matched elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .before( function )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/before
		/// </summary>
		///	<param name="content" type="jQuery">
		/// 	An element, HTML string, or jQuery object to insert before each element in the set of matched elements.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	insertAfter: function(target){
		/// <summary>
		/// 	Insert every element in the set of matched elements after the target.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/insertAfter
		/// </summary>
		///	<param name="target" type="jQuery">
		/// 	A selector, element, HTML string, or jQuery object; the matched set of elements will be inserted after the element(s) specified by this parameter.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	after: function(content){
		/// <summary>
		/// 	Insert content, specified by the parameter, after each element in the set of matched elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .after( method )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/after
		/// </summary>
		///	<param name="content" type="jQuery">
		/// 	An element, HTML string, or jQuery object to insert after each element in the set of matched elements.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	prependTo: function(target){
		/// <summary>
		/// 	Insert every element in the set of matched elements to the beginning of the target.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/prependTo
		/// </summary>
		///	<param name="target" type="jQuery">
		/// 	A selector, element, HTML string, or jQuery object; the matched set of elements will be inserted at the beginning of the element(s) specified by this parameter.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	prepend: function(content){
		/// <summary>
		/// 	Insert content, specified by the parameter, to the beginning of each element in the set of matched elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .prepend( method )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/prepend
		/// </summary>
		///	<param name="content" type="jQuery">
		/// 	An element, HTML string, or jQuery object to insert at the beginning of each element in the set of matched elements.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	appendTo: function(target){
		/// <summary>
		/// 	Insert every element in the set of matched elements to the end of the target.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/appendTo
		/// </summary>
		///	<param name="target" type="jQuery">
		/// 	A selector, element, HTML string, or jQuery object; the matched set of elements will be inserted at the end of the element(s) specified by this parameter.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	append: function(content){
		/// <summary>
		/// 	Insert content, specified by the parameter, to the end of each element in the set of matched elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .append( method )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/append
		/// </summary>
		///	<param name="content" type="jQuery">
		/// 	An element, HTML string, or jQuery object to insert at the end of each element in the set of matched elements.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	val: function(){
		/// <summary>
		/// 	Get the current value of the first element in the set of matched elements.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/val
		/// </summary>
		/// <returns type="Array" />
	}, 
	val: function(value){
		/// <summary>
		/// 	Set the value of each element in the set of matched elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .val( method )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/val
		/// </summary>
		///	<param name="value" type="String">
		/// 	A string of text or an array of strings to set as the value property of each matched element.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	text: function(){
		/// <summary>
		/// 	Get the combined text contents of each element in the set of matched elements, including their descendants.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/text
		/// </summary>
		/// <returns type="String" />
	}, 
	text: function(textString){
		/// <summary>
		/// 	Set the content of each element in the set of matched elements to the specified text.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .text( method )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/text
		/// </summary>
		///	<param name="textString" type="String">
		/// 	A string of text to set as the content of each matched element.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	html: function(){
		/// <summary>
		/// 	Get the HTML contents of the first element in the set of matched elements.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/html
		/// </summary>
		/// <returns type="String" />
	}, 
	html: function(htmlString){
		/// <summary>
		/// 	Set the HTML contents of each element in the set of matched elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .html( method )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/html
		/// </summary>
		///	<param name="htmlString" type="String">
		/// 	A string of HTML to set as the content of each matched element.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	map: function(method){
		/// <summary>
		/// 	Pass each element in the current matched set through a function, producing a new jQuery object containing the return values.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/map
		/// </summary>
		///	<param name="method" type="Function">
		/// 	A function object that will be invoked for each element in the current set.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	is: function(selector){
		/// <summary>
		/// 	Check the current matched set of elements against a selector and return true if at least one of these elements matches the selector.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/is
		/// </summary>
		///	<param name="selector" type="String">
		/// 	A string containing a selector expression to match elements against.
		/// </param>
		/// <returns type="Boolean" />
	}, 
	eq: function(index){
		/// <summary>
		/// 	Reduce the set of matched elements to the one at the specified index.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .eq( -index )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/eq
		/// </summary>
		///	<param name="index" type="Number" integer="true">
		/// 	An integer indicating the 0-based position of the element. 
		/// </param>
		/// <returns type="jQuery" />
	}, 
	filter: function(selector){
		/// <summary>
		/// 	Reduce the set of matched elements to those that match the selector or pass the function's test. 
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .filter( method )
		/// 	&#10;&#09;2. .filter( element )
		/// 	&#10;&#09;3. .filter( jQuery object )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/filter
		/// </summary>
		///	<param name="selector" type="String">
		/// 	A string containing a selector expression to match the current set of elements against.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	toggleClass: function(className){
		/// <summary>
		/// 	Add or remove one or more classes from each element in the set of matched elements, depending on either the class's presence or the value of the switch argument.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .toggleClass( className, switch )
		/// 	&#10;&#09;2. .toggleClass( method, [switch] )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/toggleClass
		/// </summary>
		///	<param name="className" type="String">
		/// 	One or more class names (separated by spaces) to be toggled for each element in the matched set.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	removeClass: function(className){
		/// <summary>
		/// 	Remove a single class, multiple classes, or all classes from each element in the set of matched elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .removeClass( method )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/removeClass
		/// </summary>
		///	<param name="className" type="String" optional="true">
		/// 	A class name to be removed from the class attribute of each matched element.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	hasClass: function(className){
		/// <summary>
		/// 	Determine whether any of the matched elements are assigned the given class.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/hasClass
		/// </summary>
		///	<param name="className" type="String">
		/// 	The class name to search for.
		/// </param>
		/// <returns type="Boolean" />
	}, 
	removeAttr: function(attributeName){
		/// <summary>
		/// 	Remove an attribute from each element in the set of matched elements.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/removeAttr
		/// </summary>
		///	<param name="attributeName" type="String">
		/// 	An attribute to remove.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	attr: function(attributeName){
		/// <summary>
		/// 	Get the value of an attribute for the first element in the set of matched elements.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/attr
		/// </summary>
		///	<param name="attributeName" type="String">
		/// 	The name of the attribute to get.
		/// </param>
		/// <returns type="String" />
	}, 
	attr: function(attributeName, value){
		/// <summary>
		/// 	Set one or more attributes for the set of matched elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .attr( map )
		/// 	&#10;&#09;2. .attr( attributeName, method )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/attr
		/// </summary>
		///	<param name="attributeName" type="String">
		/// 	The name of the attribute to set.
		/// </param>
		///	<param name="value" type="Number">
		/// 	A value to set for the attribute.
		/// </param>
		/// <returns type="jQuery" />
	}, 
	addClass: function(className){
		/// <summary>
		/// 	Adds the specified class(es) to each of the set of matched elements.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. .addClass( method )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/addClass
		/// </summary>
		///	<param name="className" type="String">
		/// 	One or more class names to be added to the class attribute of each matched element.
		/// </param>
		/// <returns type="jQuery" />
	}
};

jQuery.type = function(obj){
		/// <summary>
		/// 	Determine the internal JavaScript [[Class]] of an object.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.type
		/// </summary>
		///	<param name="obj" type="Object">
		/// 	Object to get the internal JavaScript [[Class]] of.
		/// </param>
		/// <returns type="String" />
};
jQuery.isWindow = function(obj){
		/// <summary>
		/// 	Determine whether the argument is a window.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.isWindow
		/// </summary>
		///	<param name="obj" type="Object">
		/// 	Object to test whether or not it is a window.
		/// </param>
		/// <returns type="Boolean" />
};
jQuery.error = function(message){
		/// <summary>
		/// 	Takes a string and throws an exception containing it.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.error
		/// </summary>
		///	<param name="message" type="String">
		/// 	The message to send out.
		/// </param>
};
jQuery.parseJSON = function(json){
		/// <summary>
		/// 	Takes a well-formed JSON string and returns the resulting JavaScript object.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.parseJSON
		/// </summary>
		///	<param name="json" type="String">
		/// 	The JSON string to parse.
		/// </param>
		/// <returns type="Object" />
};
jQuery.proxy = function(function, context){
		/// <summary>
		/// 	Takes a function and returns a new one that will always have a particular context.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. jQuery.proxy( context, name )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.proxy
		/// </summary>
		///	<param name="method" type="Function">
		/// 	The function whose context will be changed.
		/// </param>
		///	<param name="context" type="Object">
		/// 	The object to which the context (`this`) of the function should be set.
		/// </param>
		/// <returns type="Function" />
};
jQuery.contains = function(container, contained){
		/// <summary>
		/// 	Check to see if a DOM node is within another DOM node.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.contains
		/// </summary>
		///	<param name="container" type="Element">
		/// 	The DOM element that may contain the other element.
		/// </param>
		///	<param name="contained" type="Element">
		/// 	The DOM node that may be contained by the other element.
		/// </param>
		/// <returns type="Boolean" />
};
jQuery.noop = function(){
		/// <summary>
		/// 	An empty function.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.noop
		/// </summary>
		/// <returns type="Function" />
};
jQuery.globalEval = function(code){
		/// <summary>
		/// 	Execute some JavaScript code globally.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.globalEval
		/// </summary>
		///	<param name="code" type="String">
		/// 	The JavaScript code to execute.
		/// </param>
};
jQuery.isXMLDoc = function(node){
		/// <summary>
		/// 	Check to see if a DOM node is within an XML document (or is an XML document).
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.isXMLDoc
		/// </summary>
		///	<param name="node" type="Element">
		/// 	The DOM node that will be checked to see if it's in an XML document.
		/// </param>
		/// <returns type="Boolean" />
};
jQuery.removeData = function(element, name){
		/// <summary>
		/// 	Remove a previously-stored piece of data.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.removeData
		/// </summary>
		///	<param name="element" type="Element">
		/// 	A DOM element from which to remove data.
		/// </param>
		///	<param name="name" type="String" optional="true">
		/// 	A string naming the piece of data to remove.
		/// </param>
		/// <returns type="jQuery" />
};
jQuery.data = function(element, key, value){
		/// <summary>
		/// 	Store arbitrary data associated with the specified element.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.data
		/// </summary>
		///	<param name="element" type="Element">
		/// 	The DOM element to associate with the data.
		/// </param>
		///	<param name="key" type="String">
		/// 	A string naming the piece of data to set.
		/// </param>
		///	<param name="value" type="Object">
		/// 	The new data value.
		/// </param>
		/// <returns type="jQuery" />
};
jQuery.data = function(element, key){
		/// <summary>
		/// 	Returns value at named data store for the element, as set by jQuery.data(element, name, value), or the full data store for the element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. jQuery.data( element )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.data
		/// </summary>
		///	<param name="element" type="Element">
		/// 	The DOM element to query for the data.
		/// </param>
		///	<param name="key" type="String">
		/// 	Name of the data stored.
		/// </param>
		/// <returns type="Object" />
};
jQuery.dequeue = function(element, queueName){
		/// <summary>
		/// 	Execute the next function on the queue for the matched element.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.dequeue
		/// </summary>
		///	<param name="element" type="Element">
		/// 	A DOM element from which to remove and execute a queued function.
		/// </param>
		///	<param name="queueName" type="String" optional="true">
		/// 	A string containing the name of the queue. Defaults to fx, the standard effects queue.
		/// </param>
		/// <returns type="jQuery" />
};
jQuery.queue = function(element, queueName){
		/// <summary>
		/// 	Show the queue of functions to be executed on the matched element.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.queue
		/// </summary>
		///	<param name="element" type="Element">
		/// 	A DOM element to inspect for an attached queue.
		/// </param>
		///	<param name="queueName" type="String" optional="true">
		/// 	A string containing the name of the queue. Defaults to fx, the standard effects queue.
		/// </param>
		/// <returns type="Array" />
};
jQuery.queue = function(element, queueName, newQueue){
		/// <summary>
		/// 	Manipulate the queue of functions to be executed on the matched element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. jQuery.queue( element, queueName, method )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.queue
		/// </summary>
		///	<param name="element" type="Element">
		/// 	A DOM element where the array of queued functions is attached.
		/// </param>
		///	<param name="queueName" type="String">
		/// 	A string containing the name of the queue. Defaults to fx, the standard effects queue.
		/// </param>
		///	<param name="newQueue" type="Array">
		/// 	An array of functions to replace the current queue contents.
		/// </param>
		/// <returns type="jQuery" />
};
jQuery.isEmptyObject = function(object){
		/// <summary>
		/// 	Check to see if an object is empty (contains no properties).
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.isEmptyObject
		/// </summary>
		///	<param name="object" type="Object">
		/// 	The object that will be checked to see if it's empty.
		/// </param>
		/// <returns type="Boolean" />
};
jQuery.isPlainObject = function(object){
		/// <summary>
		/// 	Check to see if an object is a plain object (created using "{}" or "new Object").
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.isPlainObject
		/// </summary>
		///	<param name="object" type="Object">
		/// 	The object that will be checked to see if it's a plain object.
		/// </param>
		/// <returns type="Boolean" />
};
jQuery.noConflict = function(removeAll){
		/// <summary>
		/// 	Relinquish jQuery's control of the $ variable.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.noConflict
		/// </summary>
		///	<param name="removeAll" type="Boolean" optional="true">
		/// 	A Boolean indicating whether to remove all jQuery variables from the global scope (including jQuery itself).
		/// </param>
		/// <returns type="Object" />
};
jQuery.ajaxSetup = function(options){
		/// <summary>
		/// 	Set default values for future Ajax requests.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.ajaxSetup
		/// </summary>
		///	<param name="options" type="Object">
		/// 	A set of key/value pairs that configure the default Ajax request. All options are optional. 
		/// </param>
};
jQuery.post = function(url, data, method, dataType){
		/// <summary>
		/// 	Load data from the server using a HTTP POST request.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.post
		/// </summary>
		///	<param name="url" type="String">
		/// 	A string containing the URL to which the request is sent.
		/// </param>
		///	<param name="data" type="String" optional="true">
		/// 	A map or string that is sent to the server with the request.
		/// </param>
		///	<param name="method" type="Function" optional="true">
		/// 	A callback function that is executed if the request succeeds.
		/// </param>
		///	<param name="dataType" type="String" optional="true">
		/// 	The type of data expected from the server.
		/// </param>
		/// <returns type="XMLHttpRequest" />
};
jQuery.getScript = function(url, method){
		/// <summary>
		/// 	Load a JavaScript file from the server using a GET HTTP request, then execute it.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.getScript
		/// </summary>
		///	<param name="url" type="String">
		/// 	A string containing the URL to which the request is sent.
		/// </param>
		///	<param name="method" type="Function" optional="true">
		/// 	A callback function that is executed if the request succeeds.
		/// </param>
		/// <returns type="XMLHttpRequest" />
};
jQuery.getJSON = function(url, data, method){
		/// <summary>
		/// 	Load JSON-encoded data from the server using a GET HTTP request.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.getJSON
		/// </summary>
		///	<param name="url" type="String">
		/// 	A string containing the URL to which the request is sent.
		/// </param>
		///	<param name="data" type="Object" optional="true">
		/// 	A map or string that is sent to the server with the request.
		/// </param>
		///	<param name="method" type="Function" optional="true">
		/// 	A callback function that is executed if the request succeeds.
		/// </param>
		/// <returns type="XMLHttpRequest" />
};
jQuery.get = function(url, data, method, dataType){
		/// <summary>
		/// 	Load data from the server using a HTTP GET request.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.get
		/// </summary>
		///	<param name="url" type="String">
		/// 	A string containing the URL to which the request is sent.
		/// </param>
		///	<param name="data" type="String" optional="true">
		/// 	A map or string that is sent to the server with the request.
		/// </param>
		///	<param name="method" type="Function" optional="true">
		/// 	A callback function that is executed if the request succeeds.
		/// </param>
		///	<param name="dataType" type="String" optional="true">
		/// 	The type of data expected from the server.
		/// </param>
		/// <returns type="XMLHttpRequest" />
};
jQuery.ajax = function(settings){
		/// <summary>
		/// 	Perform an asynchronous HTTP (Ajax) request.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.ajax
		/// </summary>
		///	<param name="settings" type="Object">
		/// 	A set of key/value pairs that configure the Ajax request. All options are optional. A default can be set for any option with $.ajaxSetup().
		/// </param>
		/// <returns type="XMLHttpRequest" />
};
jQuery.param = function(obj){
		/// <summary>
		/// 	Create a serialized representation of an array or object, suitable for use in a URL query string or Ajax request. 
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. jQuery.param( obj, traditional )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.param
		/// </summary>
		///	<param name="obj" type="Object">
		/// 	An array or object to serialize.
		/// </param>
		/// <returns type="String" />
};
jQuery.trim = function(str){
		/// <summary>
		/// 	Remove the whitespace from the beginning and end of a string.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.trim
		/// </summary>
		///	<param name="str" type="String">
		/// 	The string to trim.
		/// </param>
		/// <returns type="String" />
};
jQuery.isFunction = function(obj){
		/// <summary>
		/// 	Determine if the argument passed is a Javascript function object. 
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.isFunction
		/// </summary>
		///	<param name="obj" type="Object">
		/// 	Object to test whether or not it is a function.
		/// </param>
		/// <returns type="Boolean" />
};
jQuery.isArray = function(obj){
		/// <summary>
		/// 	Determine whether the argument is an array.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.isArray
		/// </summary>
		///	<param name="obj" type="Object">
		/// 	Object to test whether or not it is an array.
		/// </param>
		/// <returns type="Boolean" />
};
jQuery.unique = function(array){
		/// <summary>
		/// 	Sorts an array of DOM elements, in place, with the duplicates removed. Note that this only works on arrays of DOM elements, not strings or numbers.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.unique
		/// </summary>
		///	<param name="array" type="Array">
		/// 	The Array of DOM elements.
		/// </param>
		/// <returns type="Array" />
};
jQuery.merge = function(first, second){
		/// <summary>
		/// 	Merge the contents of two arrays together into the first array. 
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.merge
		/// </summary>
		///	<param name="first" type="Array">
		/// 	The first array to merge, the elements of second added.
		/// </param>
		///	<param name="second" type="Array">
		/// 	The second array to merge into the first, unaltered.
		/// </param>
		/// <returns type="Array" />
};
jQuery.inArray = function(value, array){
		/// <summary>
		/// 	Search for a specified value within an array and return its index (or -1 if not found).
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.inArray
		/// </summary>
		///	<param name="value" type="Object">
		/// 	The value to search for.
		/// </param>
		///	<param name="array" type="Array">
		/// 	An array through which to search.
		/// </param>
		/// <returns type="Number" />
};
jQuery.map = function(array, method){
		/// <summary>
		/// 	Translate all items in an array or array-like object to another array of items.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.map
		/// </summary>
		///	<param name="array" type="Array">
		/// 	The Array to translate.
		/// </param>
		///	<param name="method" type="Function">
		/// 	The function to process each item against.  The first argument to the function is the list item, the second argument is the index in array The function can return any value.  this will be the global window object. 
		/// </param>
		/// <returns type="Array" />
};
jQuery.makeArray = function(obj){
		/// <summary>
		/// 	Convert an array-like object into a true JavaScript array.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.makeArray
		/// </summary>
		///	<param name="obj" type="Object">
		/// 	Any object to turn into a native Array.
		/// </param>
		/// <returns type="Array" />
};
jQuery.grep = function(array, method, invert){
		/// <summary>
		/// 	Finds the elements of an array which satisfy a filter function. The original array is not affected.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.grep
		/// </summary>
		///	<param name="array" type="Array">
		/// 	The array to search through.
		/// </param>
		///	<param name="method" type="Function">
		/// 	The function to process each item against.  The first argument to the function is the item, and the second argument is the index.  The function should return a Boolean value.  this will be the global window object.
		/// </param>
		///	<param name="invert" type="Boolean" optional="true">
		/// 	If "invert" is false, or not provided, then the function returns an array consisting of all elements for which "callback" returns true.  If "invert" is true, then the function returns an array consisting of all elements for which "callback" returns false.
		/// </param>
		/// <returns type="Array" />
};
jQuery.extend = function(target, object1, objectN){
		/// <summary>
		/// 	Merge the contents of two or more objects together into the first object.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. jQuery.extend( [deep], target, object1, [objectN] )
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.extend
		/// </summary>
		///	<param name="target" type="Object">
		/// 	 An object that will receive the new properties if additional objects are passed in or that will extend the jQuery namespace if it is the sole argument.
		/// </param>
		///	<param name="object1" type="Object" optional="true">
		/// 	An object containing additional properties to merge in.
		/// </param>
		///	<param name="objectN" type="Object" optional="true">
		/// 	Additional objects containing properties to merge in.
		/// </param>
		/// <returns type="Object" />
};
jQuery.each = function(collection, method){
		/// <summary>
		/// 	A generic iterator function, which can be used to seamlessly iterate over both objects and arrays. Arrays and array-like objects with a length property (such as a function's arguments object) are iterated by numeric index, from 0 to length-1. Other objects are iterated via their named properties.
		/// 	&#10;&#10;API Reference: http://api.jquery.com/jQuery.each
		/// </summary>
		///	<param name="collection" type="Object">
		/// 	The object or array to iterate over.
		/// </param>
		///	<param name="method" type="Function">
		/// 	The function that will be executed on every object.
		/// </param>
		/// <returns type="Object" />
};