import React, { Component } from 'react';

export class FetchTracks extends Component {
    static displayName = FetchTracks.name;

    constructor(props) {
        super(props);
        this.getTracks = this.getTracks.bind(this);
        this.handleChange = this.handleChange.bind(this);
        this.renderForm = this.renderForm.bind(this);
        this.state = { tracks: [], fetching: false, finished: false };
    }


    static renderForecastsTable(tracks) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>#</th>
                        <th>ID</th>
                        <th>Position</th>
                        <th>Title</th>
                        <th>Release</th>
                    </tr>
                </thead>
                <tbody>
                    {tracks.map((track, i) =>
                        <tr key={track.ID}>
                            <td>{i}</td>
                            <td>{track.id}</td>
                            <td>{track.position}</td>
                            <td>{track.title}</td>
                            <td>{track.release}</td>
                        </tr>
                    )}
                </tbody>
            </table>
        );
    }
    renderForm() {
        return (
            <div>
                <h1>Show all albums</h1>
                <p>Get the average words in an album for a given artist</p>
                <form onSubmit={this.getTracks}>
                    <span>Name:</span>
                    <input type="text" name="name" onChange={this.handleChange} />
                    <input type="submit" value="Submit" />
                </form>
            </div>
        )
    }
    render() {
        let data = this.state.tracks ? FetchTracks.renderForecastsTable(this.state.tracks) : null
        let fetching = this.state.fetching;
        let finished = this.state.finished;
        //let initial = this.state.tracks.length === 0 && !fetching && !finished;
        let noResults = finished === true && this.state.tracks.length === 0;
        let contents = data;

        return (
            <div>
                <h1 id="tabelLabel" >Tracks</h1>
                {this.renderForm()}
                {fetching && <p><em>Loading...</em></p>}
                {finished && contents}
                {noResults === true && <p><em>Loading...</em></p>}
            </div>
        );
    }
    handleChange(e) {
        this.setState({ ArtistName: e.target.value });
    }
    async getTracks(e) {
        e.preventDefault();
        const URI = 'tracks?artistname=' + this.state.ArtistName;
        this.setState({ fetching: true });
        const response = await fetch(URI);
        const data = await response.json();
        console.log(data);
        this.setState({ tracks: data, fetching: false, finished: true});

    }

}
