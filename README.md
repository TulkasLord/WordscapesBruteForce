# Wordscapes Brute Force - A fun project!!
Wordscapes Brute Force - (bot 🤖)

A fun project indeed!. 
This project help me to tackle most of the games (like: Wordscapes, WordConnect, WordCookies, etc...) with challenging words.

## In a nutshell ... 
**WordscapesBruteForce**, is just a simple .NET Core console App. written on C#. The App. searches through all the possible permutations from a given random letters and find a 
*real* word from the dictionary (English for now!).

## The fun starts here with a simple question ...
- ``Question:`` *How do I resolve and distinguish a "real" words from all those permutations?*
- ``Answer:`` **Look into a Merriam-Webster dictionary duhh. I am a genius!!!** 😏

### e.g: 
The letters ``h``, ``o``, ``m`` and ``e`` (or ``home``) have 48 permutations 
with different lengths, let's say 3 as minimun length per word.

``hom; hoe; hme; ome; home; hem; oem; hoem; hmo; moe; hmoe; heo; meo; hmeo; emo; 
hemo; eom; heom; ohm; ohe; ohme; ohem; omh; mhe; omhe; oeh; meh; omeh; emh; oemh; 
ehm; oehm; moh; mohe; moeh; mho; mhoe; mheo; eho; meho; eoh; meoh; eomh; eohm; emoh; 
emho; ehmo; ehom``

but only 2 are real word dictionary!!

``home`` and ``hoe`` 😃

## The overall logic, nothing fancy ...
A little of the basics of Combinatorial knowledge, some MIT code (🥇) ... and voila! Start with permutations with variable size and send those permutations as request to custom APIs, HTML, etc... to guarantee the existence of the real dictionary word and then save it in an local cache (JSON file) to avoid hitting too much Internet next time, acting as my first pool before doing any request. The word "home" still a word in my hard drive locally or out there in the Library of Congress API (if any) 😄

Using multi-threaded parallel (TPL) techniques with batch processing techniques, lazy loading, I guarantee that you can make thousands of simulated calls.

## Rights & Permissions about this code ... NONE!
This is free and **AS IS** you are resposible to help yourself and to do what you think is best... just make sure that it's better than the previous version  and share with others. 

### and ...
Don't worry I'm NOT going to ask for ☕ I learn how to code one! 😄

----------------------
YouTube: https://youtu.be/CV41DqRvyPw  
