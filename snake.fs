variable head
variable length
variable direction

: not ( b -- b ) true xor ;
: myrand ( a b -- r ) over - utime + swap mod + ; 

: snake-size 200 ;
: xdim 50 ;
: ydim 20 ;

create snake snake-size cells 2 * allot
create apple 2 cells allot

: segment ( seg -- adr ) 
	head @ + snake-size mod cells 2 * snake + 
;

: pos+ ( x1 y1 x2 y2 -- x y ) 
	rot + -rot + swap 
;

: point= 
	2@ rot 2@ rot = -rot = and 
;

: head* ( -- x y ) 
	0 segment \ ." +"
;

: move-head! ( -- ) 
	head @ 1 - snake-size mod head ! 
;

: grow! ( -- ) 
	1 length +! 
;

: eat-apple! ( -- )  
	1 xdim myrand 1 ydim myrand apple 2! grow! 
;

: step! ( xdiff ydiff -- ) 
	head* 2@ move-head! pos+ head* 2! 
;

\ directions
: left  -1  0 ;
: right  1  0 ;
: down   0  1 ;
: up     0 -1 ;

: wall? ( -- bool ) 
	head* 2@ 1 ydim within swap 1 xdim within and not 
;

: crossing? ( -- bool ) 
	false length @ 1 ?do 
		i segment head* point= or 
	loop 
;

: apple? ( -- bool ) 
	head* apple point= 
;

: dead? 
	wall? crossing? or 
;

: draw-frame ( -- ) 
	0 0 at-xy xdim 0 ?do 
		." *" 
	loop
	ydim 0 ?do 
		xdim i at-xy ." *" cr ." *" 
	loop 
	xdim 0 ?do 
		." *" 
	loop cr 
;

: draw-snake ( -- ) 
	length @ 0 ?do 
		i segment 2@ at-xy ." #" 
	loop 
;

: draw-apple ( -- ) 
	apple 2@ at-xy ." @" 
;

: render 
	page draw-snake draw-apple draw-frame cr length @ . 
;

: newgame!
  0 head ! xdim 2 / ydim 2 / snake 2! 3 3 apple 2! 3 length !
  ['] up direction ! left step! left step! left step! left step! 
;

: prepareexit 
	cr cr 
	." QUIT" 
	cr cr
	bye
;

: gameloop ( time -- )
	begin render dup ms
		key? if key
              		dup 113 = if ['] prepareexit else 
	   		dup 106 = if ['] left else
	   		dup 105 = if ['] up else
	   		dup 108 = if ['] right else
	   		dup 107 = if ['] down else direction @
						then 
					then 
				then 
			then 
		then
	 	direction ! drop  then
		direction perform step!
		apple? if 
			eat-apple! 
		then
		dead? 
	until 
	drop cr cr ." *** GAME OVER ***" key cr cr bye 
;

newgame! \ init

page cr 
." Snake in Forth" cr
." Press key to run game" key  \ wait for user to be ready 
150 gameloop
