import React, { Component } from 'react';
import DisplayResults from './DisplayResults';
export class RastaPage extends Component {
    constructor() {
        super();
        this.state = { ArtistName: "", result: null };
        this.searchArtist = this.searchArtist.bind(this);
        this.handleChange = this.handleChange.bind(this);
        this.getReleases = this.getReleases.bind(this);
        this.getTracks = this.getTracks.bind(this);
        this.FetchResults = this.FetchResults.bind(this);
    }
    handleChange(e) {
        this.setState({ ArtistName: e.target.value });
    }
    async FetchResults(URI) {
        const response = await fetch(URI);
        const data = await response.json();
        console.log(data);
        this.setState({ result: data });
    }
    async searchArtist(e) {
        e.preventDefault();
        const URI = 'ArtistStats?artistname=' + this.state.ArtistName;
        this.FetchResults(URI);
    }
    async getReleases(artistID) {
        const URI = 'ArtistStats?artistid=' + artistID;
        this.FetchResults(URI);
    }
    async getTracks(releaseID) {
        const URI = 'ArtistStats?releaseid=' + releaseID;
        this.FetchResults(URI);
    }
    render() {
        const result = this.state.results ? this.renderResults : <span />
        return (
            <div>
                <h1>Average words in an album</h1>
                <p>Get the average words in an album for a given artist</p>
                <form onSubmit={this.searchArtist}>
                    <span>Name:</span>
                    <input type="text" name="name" onChange={this.handleChange} />
                    <input type="submit" value="Submit" />
                </form>
                {result}
                <DisplayResults ApiData={this.state.result} GetReleases={this.getReleases} GetTracks={this.getTracks} />
            </div>
        );
    }
}
