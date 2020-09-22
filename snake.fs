variable head
variable length
variable tobeat
variable finalscore
variable direction

: not ( b -- b ) true xor ;
: myrand ( a b -- r ) over - utime + swap mod + ;  				\ random seed is not very good 

: snake-size 200 ;
: xdim 50 ;
: ydim 20 ;

\ score file management
4 constant max-line
0 value fid1									\ file ID for open/write/close functions 
variable fd-in
variable #src-fd-in
variable 'src-fd-in
Create line-buffer max-line 2 + allot
create fname 20 allot s" .score" fname place 					\ good way to assign file name variables

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
	." You choose to QUIT as a looser ... " 				\ joke to user & troll him without saving even if he made highscore
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
	'src-fd-in @ #src-fd-in @ ."      Score to beat "  type cr		\ display it  
;

: highscore? ( finalscore > fd-in -- file )
\ 	displayscoretobeat 
	." Your score " finalscore @ . cr
	'src-fd-in @ #src-fd-in @  finalscore @  < if
		cr cr ."     ***** NEW HIGH SCORE *****" cr cr 			\ save score if it is a new high score
		fname count file-status nip if i				\ fileexists ?
			fname count r/w create-file throw
		else
			fname count r/w open-file throw
		then to fid1 							\ do not forget the file ID
		finalscore @ s>d <# #s #> 					\ format score as a string
		fid1 write-line throw						\ write it on file 
		fid1 close-file throw						\ make real save of file 
	then
;

: gameloop ( time -- )
	begin render dup ms
		key? if key							\ next version should manage both uppercase & lowercase
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
		apple? if							\ if we are on apple get it in  
			eat-apple! 
		then
		dead? 								\ as it is named...
	until 
	drop cr cr ." *** GAME OVER ***" key cr cr 
	highscore?								\ check & save highscore 
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
