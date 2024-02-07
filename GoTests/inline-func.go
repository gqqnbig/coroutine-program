package main

import (
	"fmt"
)

func main() {
	c := make(chan int) // an unbuffered channel
	go func(ch chan<- int, x int) {
		ch <- x*x
	}(c, 3)

	f := func(ch chan<- int, y int) {
		ch <- y*y
		ch <- y*y
	}
	f = func(ch chan<- int, y int) {
		ch <- y*y
	}

	go f(c, 4)

	fmt.Println(<-c)
	fmt.Println(<-c)
}
