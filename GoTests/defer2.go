package main

import "fmt"

var ch1 chan int = make(chan int)
var ch2 chan bool = make(chan bool)
var ch3 chan string = make(chan string)

func s(v int) {
	defer func() {
		defer d()
		ch2 <- true
	}()
	ch1 <- v
}

func d() {
	ch3 <- "hello"
}

func main() {
	go s(2)

	fmt.Println(<-ch1, <-ch2, <-ch3)
}

