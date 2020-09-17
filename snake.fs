variable head
variable length
variable tobeat
variable finalscore
variable direction

: not ( b -- b ) true xor ;
: myrand ( a b -- r ) over - utime + swap mod + ; 

: snake-size 200 ;
: xdim 50 ;
: ydim 20 ;

\ score file management
4 constant max-line
0 value fd-out
variable fd-in
variable #src-fd-in
variable 'src-fd-in
Create line-buffer max-line 2 + allot

\ create snake & apple position grid 
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
	0 segment  
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
	page draw-snake draw-apple draw-frame cr ."      Score : " length @ dup finalscore ! . 
;

: newgame!
  0 head ! xdim 2 / ydim 2 / snake 2! 3 3 apple 2! 3 length !
  ['] up direction ! left step! left step! left step! left step! 
;

: prepareexit 	\ no score save what ever it is
	cr cr 
	." You choose to QUIT as a looser ... " 
	cr cr
	cr cr ." *** GAME OVER ***" key cr cr 
	bye
;


: displayscoretobeat
	here 'src-fd-in ! 							\ ram position
	s" .score" r/o open-file throw fd-in !
	here 4 fd-in @ read-file throw 
	dup allot								\ one alloc = 1 line
	fd-in @ close-file throw						\ now close file
	here 'src-fd-in @ - #src-fd-in ! 					\ get allocated
	'src-fd-in @ #src-fd-in @ ."      Score to beat "  type cr					\ display it  
;

: highscore? ( finalscore > fd-in -- file )
\ 	displayscoretobeat 
	." Your score " finalscore @ . cr
	'src-fd-in @ #src-fd-in @  finalscore @  < if
\		s" .score" w/o open-file throw to fd-out
\		finalscore @ . to fd-out 
		\ fd-out @ write-line throw 
\		 fd-out write-line throw 
\		." @ @ write"
\		fd-out @ close-file throw						\ now close file
	then
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
	drop cr cr ." *** GAME OVER ***" key cr cr 
	highscore?
	bye 
;

page cr cr
."      *** Snake in Forth ***" cr cr 
."      Use           i         for going up" cr 
."                j       l     for going left or right" cr
."                    k         for going down" cr cr cr 
."      You can olso in game press q to quit before the end" cr cr
."      Press key to run game" cr cr 						\ wait for user to be ready 
displayscoretobeat key
newgame! 									\ init
125 gameloop
